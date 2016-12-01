#I @"../packages"
#r @"FSharp.Data.2.3.2/lib/net40/FSharp.Data.dll"
#load "GameLibrary.fs"

open GameLibrary
open System

let rooms = Rooms.rooms
let initialRoom = 
    rooms.InitialRoom |> Rooms.GetRoom 
initialRoom |> Game.PrintInfo 
let numberOfRoomsVisited = 
    Console.ReadLine() |> Game.ProcessQuery initialRoom 1
printfn "Number of rooms visited: %d" numberOfRoomsVisited
Async.RunSynchronously Game.exitMessage