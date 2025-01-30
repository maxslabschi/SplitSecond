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

func GetScores(level string, limit int) ([]model.Score, error) {
	rows, err := db.Query("SELECT username, time, date FROM scores WHERE level=? ORDER BY time LIMIT ?", level, limit)
	if err != nil {
		return nil, err
	}
	defer rows.Close()
	scoreList := make([]model.Score, 0)
	for rows.Next() {
		score := &model.Score{}
		err := rows.Scan(&score.Username, &score.Time, &score.Date)
		if err != nil {
			return nil, err
		}
		scoreList = append(scoreList, *score)
	}
	return scoreList, nil
}
