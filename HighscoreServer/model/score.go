package model

import (
	"net/http"
	"time"
)

type Score struct {
	Username string `json:"username"`
	// time in seconds
	Time float64   `json:"time"`
	Date time.Time `json:"date"`
}

func (rd Score) Render(w http.ResponseWriter, r *http.Request) error {
	return nil
}

type ScoreCreateRequest struct {
	Username string `json:"username"`
	// time in seconds
	Time float64 `json:"time"`
}

func (a *ScoreCreateRequest) Bind(r *http.Request) error {
	return nil
}
