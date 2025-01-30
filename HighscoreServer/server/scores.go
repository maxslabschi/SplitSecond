package server

import (
	"context"
	"fmt"
	"github.com/go-chi/chi/v5"
	"github.com/go-chi/render"
	"github.com/splitsecond/db"
	"github.com/splitsecond/model"
	"net/http"
)

func ScoreRouter(r chi.Router) {
	r.Route("/{levelId}", func(r chi.Router) {
		r.Use(LevelCtx)
		r.Post("/", createScore)
	})
}

func createScore(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")

	data := &model.ScoreCreateRequest{}
	if err := render.Bind(r, data); err != nil {
		fmt.Println(err)
		render.Render(w, r, ErrInvalidRequest(err))
		return
	}

	score, err := db.CreateScore(r.Context().Value("levelId").(string), data)
	if err != nil {
		fmt.Println(err)
		// TODO: internal server error
		render.Render(w, r, ErrInvalidRequest(err))
		return
	}

	render.Render(w, r, score)
	return
}

func LevelCtx(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		levelID := chi.URLParam(r, "levelId")
		ctx := context.WithValue(r.Context(), "levelId", levelID)
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}
