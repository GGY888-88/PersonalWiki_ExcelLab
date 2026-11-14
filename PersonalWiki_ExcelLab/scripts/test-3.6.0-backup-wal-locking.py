#!/usr/bin/env python3
"""Python SQLite WAL/online-backup fixture; not a Microsoft.Data.Sqlite runtime test."""
import sqlite3,tempfile,os,shutil
checks=0
def ok(v,m):
 global checks
 checks+=1
 if not v: raise AssertionError(m)
td=tempfile.mkdtemp(prefix='pw-wal-test-'); src=os.path.join(td,'src.db'); dst=os.path.join(td,'backup.db')
try:
 c=sqlite3.connect(src); mode=c.execute('PRAGMA journal_mode=WAL').fetchone()[0]
 ok(mode.lower()=='wal','WAL enabled')
 c.execute('CREATE TABLE t(id INTEGER PRIMARY KEY,v TEXT)'); c.commit()
 ok(c.execute('SELECT COUNT(*) FROM t').fetchone()[0]==0,'empty table created')
 c.execute('INSERT INTO t(v) VALUES(?)',('committed-in-wal',)); c.commit()
 ok(c.execute('SELECT COUNT(*) FROM t').fetchone()[0]==1,'transaction committed')
 ok(c.execute('SELECT v FROM t').fetchone()[0]=='committed-in-wal','committed value readable')
 d=sqlite3.connect(dst); c.backup(d); d.close()
 b=sqlite3.connect(dst)
 ok(b.execute('SELECT v FROM t').fetchone()[0]=='committed-in-wal','online backup includes committed row')
 ok(b.execute('PRAGMA integrity_check').fetchone()[0]=='ok','backup integrity')
 ok(b.execute('SELECT COUNT(*) FROM t').fetchone()[0]==1,'backup row count')
 b.close()
 for i in range(10): c.execute('INSERT INTO t(v) VALUES(?)',(f'row-{i}',))
 c.commit()
 ok(c.execute('SELECT COUNT(*) FROM t').fetchone()[0]==11,'second transaction committed')
 dst2=os.path.join(td,'backup2.db'); d=sqlite3.connect(dst2); c.backup(d); d.close()
 b2=sqlite3.connect(dst2)
 ok(b2.execute('SELECT COUNT(*) FROM t').fetchone()[0]==11,'second backup has all rows')
 ok(b2.execute('PRAGMA integrity_check').fetchone()[0]=='ok','second backup integrity')
 b2.close()
 ro=sqlite3.connect(f'file:{src}?mode=ro',uri=True)
 try: ro.execute("INSERT INTO t(v) VALUES('x')"); readonly=False
 except sqlite3.OperationalError: readonly=True
 ok(readonly,'read-only connection blocks writes')
 ro.close()
 ok(c.execute('PRAGMA integrity_check').fetchone()[0]=='ok','source integrity')
 ok(os.path.exists(src),'source preserved')
 ok(os.path.getsize(dst)>0 and os.path.getsize(dst2)>0,'backup files nonempty')
 code=open('Services/DatabaseBackupService.cs',encoding='utf-8-sig').read()
 ok('HasSqliteFiles' in code,'source checks SQLite sidecars')
 ok('File.Move(staging, target)' in code,'no-overwrite final move')
 ok('CleanupSqliteFiles(staging)' in code,'staging cleanup')
 print(f'PASS {checks}/17')
finally:
 try: c.close()
 except: pass
 shutil.rmtree(td,ignore_errors=True)
