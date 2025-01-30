package server

import (
	"context"
	"github.com/go-chi/chi/v5"
	"github.com/go-chi/render"
	"github.com/splitsecond/db"
	"github.com/splitsecond/model"
	"log"
	"net/http"
	"strconv"
)

func ScoreRouter(r chi.Router) {
	r.Route("/{levelId}/scores", func(r chi.Router) {
		r.Use(LevelCtx)
		r.Post("/", createScore)
		r.Get("/", getScores)
	})
}

func createScore(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")

	data := &model.ScoreCreateRequest{}
	if err := render.Bind(r, data); err != nil {
		log.Println(err)
		render.Render(w, r, ErrInvalidRequest(err))
		return
	}

	score, err := db.CreateScore(r.Context().Value("levelId").(string), data)
	if err != nil {
		log.Println(err)
		render.Render(w, r, ErrInvalidRequest(err))
		return
	}

	render.Render(w, r, score)
	return
}

func getScores(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")

	limitStr := r.URL.Query().Get("limit")
	if limitStr == "" {
		limitStr = "100"
	}
	limit, err := strconv.Atoi(limitStr)
	if err != nil {
		render.Render(w, r, ErrInvalidRequest(err))
		return
	}

	data, err := db.GetScores(r.Context().Value("levelId").(string), limit)
	if err != nil {
		log.Println(err)
		render.Render(w, r, ErrInternalServerError(err))
		return
	}
	renderers := make([]render.Renderer, len(data))
	for i, score := range data {
		renderers[i] = score
	}
	render.RenderList(w, r, renderers)
}

func LevelCtx(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		levelID := chi.URLParam(r, "levelId")
		ctx := context.WithValue(r.Context(), "levelId", levelID)
		next.ServeHTTP(w, r.WithContext(ctx))
	})
}
