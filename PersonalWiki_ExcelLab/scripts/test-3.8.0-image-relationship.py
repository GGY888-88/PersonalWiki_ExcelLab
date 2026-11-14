#!/usr/bin/env python3
"""SQLite fixture and source-contract checks for 3.8.0 image and relationship improvements."""
import hashlib, sqlite3, tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
checks = 0

def ok(value, message):
    global checks
    checks += 1
    if not value:
        raise AssertionError(message)

with tempfile.TemporaryDirectory() as temp:
    db = Path(temp) / 'fixture.db'
    image = Path(temp) / 'sample.png'
    image.write_bytes(b'personalwiki-image-fixture')
    digest = hashlib.sha256(image.read_bytes()).hexdigest()
    c = sqlite3.connect(db)
    c.executescript('''
      CREATE TABLE entries(id TEXT PRIMARY KEY,title TEXT,entry_type TEXT,summary TEXT,updated_at TEXT,is_archived INTEGER);
      CREATE TABLE entry_tags(entry_id TEXT,tag TEXT,normalized_tag TEXT);
      CREATE TABLE entry_images(id TEXT PRIMARY KEY,entry_id TEXT,file_path TEXT,sha256 TEXT,caption TEXT,sort_order INTEGER,created_at TEXT, UNIQUE(entry_id,file_path));
      INSERT INTO entries VALUES('source','Source','人物',NULL,'2026-01-01',0);
      INSERT INTO entries VALUES('related','Related','人物',NULL,'2026-01-02',0);
      INSERT INTO entries VALUES('archived','Archived','人物',NULL,'2026-01-03',1);
      INSERT INTO entry_tags VALUES('source','收藏','收藏');
      INSERT INTO entry_tags VALUES('related','收藏','收藏');
      INSERT INTO entry_tags VALUES('archived','收藏','收藏');
    ''')
    c.execute('INSERT INTO entry_images VALUES(?,?,?,?,?,?,?)', ('img1','source',str(image),digest,'old caption',0,'2026-01-01'))
    c.commit()
    c.execute('UPDATE entry_images SET caption=? WHERE id=?', ('new caption','img1'))
    ok(c.execute('SELECT caption FROM entry_images WHERE id="img1"').fetchone()[0] == 'new caption', 'caption update persisted')
    ok(image.exists(), 'caption update does not touch original file')
    ok(hashlib.sha256(image.read_bytes()).hexdigest() == digest, 'original image hash unchanged')
    rel = c.execute('''SELECT e.id,COUNT(DISTINCT shared.normalized_tag),COALESCE(GROUP_CONCAT(DISTINCT shared.tag),'')
      FROM entries source JOIN entries e ON e.id<>source.id AND e.is_archived=0
      LEFT JOIN entry_tags shared ON shared.entry_id=e.id AND shared.normalized_tag IN
        (SELECT normalized_tag FROM entry_tags WHERE entry_id='source')
      WHERE source.id='source' GROUP BY e.id HAVING COUNT(DISTINCT shared.normalized_tag)>0''').fetchall()
    ok(rel == [('related',1,'收藏')], 'related entries expose shared tags and exclude archived entries')
    c.execute('PRAGMA query_only=ON')
    try:
        c.execute("UPDATE entry_tags SET tag='changed'")
        readonly = False
    except sqlite3.OperationalError:
        readonly = True
    ok(readonly, 'relationship browsing remains read-only')
    c.close()

service = (ROOT/'Services/EntryContentService.cs').read_text(encoding='utf-8-sig')
for token in ['UpdateImageCaption','VerifyImageFile','SHA256.HashData(stream)','SqliteOpenMode.ReadOnly','FileShare.ReadWrite | FileShare.Delete','OriginalFileChanged = false']:
    ok(token in service, f'image service contract: {token}')
related = (ROOT/'Services/RelatedEntryService.cs').read_text(encoding='utf-8-sig')
for token in ['GROUP_CONCAT(DISTINCT shared.tag)','SqliteOpenMode.ReadOnly','e.is_archived=0','SharedTags']:
    ok(token in related, f'related-entry contract: {token}')
ui = (ROOT/'PersonalWiki.Desktop/MainForm.cs').read_text(encoding='utf-8-sig')
for token in ['編輯圖片說明','檢查圖片檔案','UpdateImageCaption','VerifyImageFile','共用標籤數','item.SharedTags']:
    ok(token in ui, f'desktop UI contract: {token}')

schema = (ROOT/'Services/WikiDatabaseService.cs').read_text(encoding='utf-8-sig')
ok('PRAGMA user_version=1;' in schema, 'schema version remains 1')
ok('CREATE TABLE entry_images' in schema and 'CREATE TABLE entry_tags' in schema, 'existing image and tag schema retained')
print(f'PASS {checks}/{checks}')
