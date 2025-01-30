package main

import (
	"github.com/splitsecond/db"
	"github.com/splitsecond/server"
	"log"
)

func main() {
	db.ConnectToDatabase()
	log.Println("Opened DB")
	err := server.StartServer()
	if err != nil {
		log.Panic(err)
	}
	log.Println("Goodbye!")
}
