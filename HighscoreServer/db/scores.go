package db

import (
	"github.com/splitsecond/model"
	"time"
)

func CreateScore(level string, scoreCreate *model.ScoreCreateRequest) (*model.Score, error) {
	score := model.Score{
		Username: scoreCreate.Username,
		Time:     scoreCreate.Time,
		Date:     time.Now(),
	}

	statement := `
		INSERT INTO scores (level, username, time, date)
		VALUES (?, ?, ?, ?)
	`

	_, err := db.Exec(statement, level, score.Username, score.Time, score.Date)
	if err != nil {
		return nil, err
	}

	return &score, err
}
