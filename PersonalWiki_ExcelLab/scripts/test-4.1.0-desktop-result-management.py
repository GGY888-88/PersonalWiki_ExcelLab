#!/usr/bin/env python3
"""4.1.0 static contract checks for safe multi-select result-summary copying."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
source = (ROOT / 'PersonalWiki.Desktop/MainForm.cs').read_text(encoding='utf-8-sig')
checks = 0

def ok(condition, message):
    global checks
    checks += 1
    if not condition:
        raise AssertionError(message)

for token in (
    'Text = "複製選取摘要"',
    'CopySelectedResultSummaries()',
    '_copySelectedResults.Enabled = _results.SelectedItems.Count > 0',
    '_results.SelectedItems.Cast<ListViewItem>().ToList()',
    'Clipboard.SetText(',
    '名稱：{row.Text}',
    '類型：{row.SubItems[1].Text}',
    '摘要：{row.SubItems[2].Text}',
    '更新時間：{row.SubItems[3].Text}',
    '狀態：{row.SubItems[4].Text}',
    'ID：{row.Tag?.ToString() ?? string.Empty}',
    '資料庫未變更。',
    'catch (Exception ex)',
):
    ok(token in source, f'Missing desktop result-copy contract: {token}')

# This feature must remain presentation-only: no database write service may be called in the copy handler.
start = source.index('private void CopySelectedResultSummaries()')
end = source.index('private void SearchEntries()', start)
handler = source[start:end]
for forbidden in ('SetArchived(', 'Update(', 'Delete(', 'ExecuteNonQuery', 'EntryService().'):
    ok(forbidden not in handler, f'Copy handler must not mutate data: {forbidden}')

for project in ('PersonalWiki.ExcelLab.csproj', 'PersonalWiki.Desktop/PersonalWiki.Desktop.csproj'):
    ok('<Version>4.4.0</Version>' in (ROOT / project).read_text(encoding='utf-8-sig'), f'{project} current release version mismatch')

print(f'PASS {checks}/{checks}')
