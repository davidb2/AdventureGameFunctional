namespace GameLibrary

module Rooms = 
    open FSharp.Data

    type RoomCollection = JsonProvider<"rooms.json"> 
    
    let private allRooms = 
        RoomCollection.GetSample().Rooms
        |> Array.map (fun room -> room.Name, room)
        |> Map.ofArray

    let rooms = RoomCollection.GetSample()

    let GetRoom roomName = 
        match roomName with
        | validRoomName when allRooms.ContainsKey validRoomName -> 
            allRooms.Item roomName
        | invalidRoomName ->
            failwithf "%s is not a valid room name." invalidRoomName

    type RoomCollection.Room with
        member private this.Neighbors = 
            this.Directions
            |> Array.map (fun dir -> dir.Direction.ToLower(), dir.Room)
            |> Map.ofArray
        member this.HasNeighbor roomName = 
            this.Neighbors.ContainsKey roomName
        member this.GetNeighbor direction =
            match direction with
            | validDirection when this.Neighbors.ContainsKey validDirection ->
                this.Neighbors.Item validDirection
            | invalidDirection -> 
                failwithf "%s is not a valid direction for %s."
                    invalidDirection
                    this.Name
module Game = 
    open FSharp.Data
    open System
    open System.Threading
    open Rooms

    let sleepTime = 750

    let isExitWord = function
    | word when 
        word = "exit" || 
        word = "quit" ||
        word = "done" ||
        word = "finish" -> true
    | _ -> false 

    let PrintInfo (room : RoomCollection.Room) = 
        let directions = 
            room.Directions
            |> Array.map (fun dir -> dir.Direction)
        printfn "%s\nFrom here, you can go: %s" 
            room.Description 
            (String.Join(", ", directions))

    let exitMessage = async {    
        printf "Exiting "
        do! Async.Sleep sleepTime
        printf ". "
        do! Async.Sleep sleepTime
        printf ". "
        do! Async.Sleep sleepTime
        printf ". "
        }

    let rec ProcessQuery (currentRoom : RoomCollection.Room) roomCount (query : string) = 
        let words = 
            query
                .ToLower()
                .Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
            |> Array.toList
        match words with
        | [] -> 
            Console.ReadLine() |> ProcessQuery currentRoom roomCount
//        | _ when ContainsProfanity query ->
//            printfn "You are being rather rude to me. Goodbye." 
//            Async.RunSynchronously exitMessage
//            roomCount
        | [cmd] when isExitWord cmd -> 
            roomCount
        | cmd::direction::otherStuff when cmd = "go" ->
            match direction with
            | direction when direction |> currentRoom.HasNeighbor && otherStuff.IsEmpty ->
                // TODO: convert string to room
                let newRoom = currentRoom.GetNeighbor direction |> Rooms.GetRoom
                PrintInfo newRoom
                Console.ReadLine() |> ProcessQuery newRoom (roomCount + 1)
            | invalidDirection -> 
                printfn "I am sorry. I cannot go '%s'." (query.Split([|' '|], 2).[1])
                PrintInfo currentRoom
                Console.ReadLine() |> ProcessQuery currentRoom roomCount
        | invalidQuery -> 
            printfn "I am sorry. I cannot process '%s'." query
            PrintInfo currentRoom
            Console.ReadLine() |> ProcessQuery currentRoom roomCount