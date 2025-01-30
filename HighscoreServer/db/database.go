package db

import "database/sql"
import _ "github.com/mattn/go-sqlite3"

var db *sql.DB

func ConnectToDatabase() {
	var err error

	db, err = sql.Open("sqlite3", "./data/database.db")
	if err != nil {
		panic(err)
	}

	_, err = db.Exec("CREATE TABLE IF NOT EXISTS scores (level VARCHAR, username VARCHAR, time REAL, date DATE)")
	if err != nil {
		panic(err)
	}

	_, err = db.Exec("CREATE INDEX IF NOT EXISTS scores_level ON scores(level);")
	if err != nil {
		panic(err)
	}
}
