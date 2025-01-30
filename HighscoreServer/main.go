package main

import (
	"github.com/splitsecond/db"
	"github.com/splitsecond/server"
)

func main() {
	db.ConnectToDatabase()
	println("Connected")
	err := server.StartServer()
	if err != nil {
		panic(err)
	}
	println("Goodbye!")
}
