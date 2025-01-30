package server

import (
	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"
	"net/http"
)

func StartServer() error {
	r := chi.NewRouter()

	r.Use(middleware.Logger)
	r.Route("/level", func(r chi.Router) {
		ScoreRouter(r)
	})

	return http.ListenAndServe(":8080", r)
}
