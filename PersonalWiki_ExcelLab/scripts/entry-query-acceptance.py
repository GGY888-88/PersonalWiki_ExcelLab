#!/usr/bin/env python3
"""Run real .NET CLI search/benchmark parity acceptance with resumable diagnostics."""
import argparse, datetime as dt, json, shlex, subprocess, sys, tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SORTS = ('updated-desc', 'title-asc', 'title-desc', 'created-desc', 'created-asc')


def get_ci(doc, key, default=None):
    target = key.casefold()
    for name, value in doc.items():
        if name.casefold() == target:
            return value
    return default


def compare(search_doc, benchmark_doc):
    search_ids = [str(get_ci(item, 'id')) for item in get_ci(search_doc, 'items', [])]
    benchmark_ids = [str(item) for item in get_ci(benchmark_doc, 'resultIds', [])]
    checks = {
        'totalCount': (get_ci(search_doc, 'totalCount'), get_ci(benchmark_doc, 'totalCount')),
        'returnedCount': (len(search_ids), get_ci(benchmark_doc, 'returnedCount')),
        'resultIds': (search_ids, benchmark_ids), 'page': (get_ci(search_doc, 'page'), get_ci(benchmark_doc, 'page')),
        'pageSize': (get_ci(search_doc, 'pageSize'), get_ci(benchmark_doc, 'pageSize')),
        'sort': (get_ci(search_doc, 'sort'), get_ci(benchmark_doc, 'sort')),
        'archivedMode': (get_ci(search_doc, 'archivedMode'), get_ci(benchmark_doc, 'archivedMode')),
        'keyword': (get_ci(search_doc, 'keyword'), get_ci(benchmark_doc, 'keyword')),
        'entryType': (get_ci(search_doc, 'entryType'), get_ci(benchmark_doc, 'entryType')),
        'tag': (get_ci(search_doc, 'tag'), get_ci(benchmark_doc, 'tag')),
        'createdFrom': (get_ci(search_doc, 'createdFrom'), get_ci(benchmark_doc, 'createdFrom')),
        'createdTo': (get_ci(search_doc, 'createdTo'), get_ci(benchmark_doc, 'createdTo')),
        'updatedFrom': (get_ci(search_doc, 'updatedFrom'), get_ci(benchmark_doc, 'updatedFrom')),
        'updatedTo': (get_ci(search_doc, 'updatedTo'), get_ci(benchmark_doc, 'updatedTo')),
    }
    return [{'field': k, 'passed': a == b, 'search': a, 'benchmark': b} for k, (a, b) in checks.items()]


def invoke(command, cwd):
    p = subprocess.run(command, cwd=cwd, text=True, encoding='utf-8', errors='replace',
                       stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if p.returncode:
        raise RuntimeError(f'CLI exit {p.returncode}: {p.stderr.strip() or p.stdout.strip()}')
    return p.stdout


def build_cases():
    cases = [{'sort':s,'page':p,'pageSize':20,'archived':'include'} for s in SORTS for p in (1,2)]
    cases.extend([
        {'sort':'updated-desc','page':1,'pageSize':20,'archived':'exclude','type':'人物','tag':'測試'},
        {'sort':'created-asc','page':1,'pageSize':20,'archived':'only'},
        {'sort':'title-asc','page':1,'pageSize':20,'archived':'include','created-from':'2026-01-01','created-to':'2026-01-20'},
        {'sort':'created-desc','page':1,'pageSize':20,'archived':'exclude','updated-from':'2026-02-01','updated-to':'2026-02-20'},
        {'sort':'updated-desc','page':1,'pageSize':20,'archived':'exclude','keyword':'測試條目'},
    ])
    return cases


def write_reports(output, report):
    (output/'acceptance-comparison.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    lines = ['# CLI Search / Benchmark Acceptance', '', f"- Version: {report['version']}",
             f"- Cases: {report['caseCount']}", f"- Passed: {report['passedCases']}",
             f"- Failed: {report['failedCases']}", f"- Not run: {report['notRunCases']}", '',
             '| # | Case | Result | Diagnostic |', '|---:|---|---|---|']
    for x in report['cases']:
        c=x['case']; label=f"{c['sort']} / page {c['page']}"
        diag=(x.get('error') or 'All parity checks passed').replace('|','\\|').replace('\n',' ')
        lines.append(f"| {x['index']} | {label} | {'PASS' if x['passed'] else 'FAIL'} | {diag} |")
        if not x['passed']:
            lines.extend(['', f"**Rerun case {x['index']}:**", '```powershell', x['rerunCommand'], '```'])
    (output/'acceptance-comparison.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')


def main():
    ap=argparse.ArgumentParser(description=__doc__)
    ap.add_argument('--database',type=Path,help='Database to test; copied to an isolated temporary folder')
    ap.add_argument('--output',type=Path,help='New output directory; it must not already exist')
    ap.add_argument('--self-test',action='store_true',help='Test comparison logic without .NET')
    ap.add_argument('--no-build',action='store_true',help='Skip restore/build (requires existing Release build)')
    ap.add_argument('--case-index',type=int,help='Run only one 1-based case from the standard 15-case suite')
    args=ap.parse_args()
    if args.self_test:
        a={'TotalCount':2,'Items':[{'Id':'a'},{'Id':'b'}],'Page':1,'PageSize':20,'Sort':'updated-desc','ArchivedMode':'exclude','Keyword':None,'EntryType':None,'Tag':None,'CreatedFrom':None,'CreatedTo':None,'UpdatedFrom':None,'UpdatedTo':None}
        b={'TotalCount':2,'ReturnedCount':2,'ResultIds':['a','b'],'Page':1,'PageSize':20,'Sort':'updated-desc','ArchivedMode':'exclude','Keyword':None,'EntryType':None,'Tag':None,'CreatedFrom':None,'CreatedTo':None,'UpdatedFrom':None,'UpdatedTo':None}
        assert all(x['passed'] for x in compare(a,b))
        b['ResultIds']=['b','a']; assert not all(x['passed'] for x in compare(a,b))
        assert len(build_cases())==15
        print('PASS acceptance comparator and case-suite self-test'); return 0
    if not args.database: ap.error('--database is required unless --self-test is used')
    if args.case_index is not None and not 1 <= args.case_index <= 15: ap.error('--case-index must be between 1 and 15')
    db=args.database.resolve()
    if not db.is_file(): ap.error(f'database not found: {db}')
    output=args.output.resolve() if args.output else ROOT.parent/f'PersonalWiki_4.6.0_Acceptance_{dt.datetime.now():%Y%m%d_%H%M%S}'
    if output.exists(): ap.error(f'refusing to reuse existing output directory: {output}')
    output.mkdir(parents=True)
    cases=build_cases()
    selected=[(i,c) for i,c in enumerate(cases,1) if args.case_index is None or i==args.case_index]
    report={'version':'4.6.0','generatedAt':dt.datetime.now(dt.timezone.utc).isoformat(),
            'databaseSource':str(db),'databaseTested':'isolated byte-for-byte copy','readOnlyExpectation':True,
            'caseCount':len(selected),'suiteCaseCount':len(cases),'passedCases':0,'failedCases':0,
            'notRunCases':0,'cases':[]}
    try:
        with tempfile.TemporaryDirectory(prefix='pw46-accept-') as td:
            isolated_db=Path(td)/db.name; isolated_db.write_bytes(db.read_bytes())
            if not args.no_build:
                invoke(['dotnet','restore','PersonalWiki.ExcelLab.csproj'],ROOT)
                invoke(['dotnet','build','PersonalWiki.ExcelLab.csproj','-c','Release','--no-restore'],ROOT)
            dll=ROOT/'bin'/'Release'/'net10.0'/'PersonalWiki.ExcelLab.dll'
            if not dll.is_file(): raise RuntimeError(f'CLI build output not found: {dll}')
            for n,case in selected:
                stem=f'case-{n:02d}-{case["sort"]}-p{case["page"]}'
                search_path=output/f'{stem}-search.json'; benchmark_path=output/f'{stem}-benchmark.json'
                qa=['entry-query',str(isolated_db),'--sort',case['sort'],'--page',str(case['page']),'--page-size',str(case['pageSize']),'--archived',case['archived'],'--output',str(search_path)]
                ba=['entry-query-benchmark',str(isolated_db),'--sort',case['sort'],'--page',str(case['page']),'--page-size',str(case['pageSize']),'--archived',case['archived'],'--iterations','3']
                if 'keyword' in case: qa.insert(2,case['keyword']); ba.insert(2,case['keyword'])
                for opt in ('type','tag','created-from','created-to','updated-from','updated-to'):
                    if opt in case: qa.extend(['--'+opt,case[opt]]); ba.extend(['--'+opt,case[opt]])
                search_cmd=['dotnet',str(dll),*qa]; benchmark_cmd=['dotnet',str(dll),*ba]
                rerun_out=f'Acceptance_4.6.0_Case{n}_Rerun'
                rerun=['python','scripts/entry-query-acceptance.py','--database',str(db),'--case-index',str(n),'--output',rerun_out]
                if args.no_build: rerun.append('--no-build')
                item={'index':n,'case':case,'passed':False,'commands':{'search':search_cmd,'benchmark':benchmark_cmd},
                      'rerunCommand':' '.join(shlex.quote(v) for v in rerun)}
                try:
                    invoke(search_cmd,ROOT); raw=invoke(benchmark_cmd,ROOT)
                    benchmark_path.write_text(raw,encoding='utf-8')
                    sdoc=json.loads(search_path.read_text(encoding='utf-8-sig')); bdoc=json.loads(raw)
                    checks=compare(sdoc,bdoc); item.update({'checks':checks,'passed':all(x['passed'] for x in checks),
                        'benchmarkMs':{k:get_ci(bdoc,k) for k in ('minimumMilliseconds','medianMilliseconds','averageMilliseconds','maximumMilliseconds')}})
                    if not item['passed']: item['error']='Parity mismatch: '+', '.join(x['field'] for x in checks if not x['passed'])
                except Exception as exc:
                    item.update({'error':f'{type(exc).__name__}: {exc}'})
                report['cases'].append(item)
                report['passedCases']=sum(x['passed'] for x in report['cases'])
                report['failedCases']=sum(not x['passed'] for x in report['cases'])
                report['notRunCases']=len(selected)-len(report['cases'])
                write_reports(output,report)
    except Exception as exc:
        report['setupError']=f'{type(exc).__name__}: {exc}'
        report['notRunCases']=len(selected)-len(report['cases'])
        write_reports(output,report)
        print(f"SETUP ERROR: {report['setupError']}\nPartial report: {output}",file=sys.stderr)
        return 1
    print(f"Acceptance: {report['passedCases']}/{report['caseCount']} passed, {report['failedCases']} failed; report: {output}")
    return 0 if report['failedCases']==0 else 2

if __name__=='__main__': raise SystemExit(main())
