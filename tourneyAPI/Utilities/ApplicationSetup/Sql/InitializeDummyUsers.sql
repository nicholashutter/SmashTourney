-- Development dummy-user seed data
-- Username pattern: dummy01 .. dummy16
-- Password pattern: DummyPass!01 .. DummyPass!16
-- Example pair: dummy01 / DummyPass!01

CREATE TABLE IF NOT EXISTS AppRuntimeConfig (
    [Key] TEXT PRIMARY KEY,
    [Value] TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS TestUserSeed (
    UserName TEXT PRIMARY KEY,
    Password TEXT NOT NULL
);

INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy01', 'DummyPass!01');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy02', 'DummyPass!02');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy03', 'DummyPass!03');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy04', 'DummyPass!04');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy05', 'DummyPass!05');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy06', 'DummyPass!06');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy07', 'DummyPass!07');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy08', 'DummyPass!08');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy09', 'DummyPass!09');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy10', 'DummyPass!10');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy11', 'DummyPass!11');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy12', 'DummyPass!12');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy13', 'DummyPass!13');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy14', 'DummyPass!14');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy15', 'DummyPass!15');
INSERT OR IGNORE INTO TestUserSeed (UserName, Password) VALUES ('dummy16', 'DummyPass!16');
