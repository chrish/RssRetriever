create table Source(
	id INTEGER PRIMARY KEY,
	name TEXT,
	url TEXT,
	ttl INTEGER,
	lastFetched INTEGER,
	active TEXT, 
	priority INTEGER,
	unique(name, url)
);

create table Story(
	id INTEGER PRIMARY KEY,
	source_id INTEGER,
	title text,
	description text,
	url text,
	unique(title, url),
	FOREIGN KEY (source_id) REFERENCES Source (id)
);

create table SourceLog(
	id INTEGER PRIMARY KEY,
	source_id INTEGER,
	message TEXT,
	ts DATETIME DEFAULT CURRENT_TIMESTAMP,
	FOREIGN KEY (source_id) REFERENCES Source (id)
);