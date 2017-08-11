module FacebookMessengerConnector.Program

open System
open System.Net
open canopy
open canopy.core
open runner
open System
open OpenQA.Selenium
open FSharp.Data
open SendAPI
open System.Diagnostics
open MessageParsing


[<EntryPoint>]
let main argv =   
  while true do
    Console.ForegroundColor <- ConsoleColor.Green 
    Console.Write(">: ")   
    Console.ForegroundColor <- ConsoleColor.White 
    let input = Console.ReadLine()
    printfnT "Received your request ..."
    printfnT "Processing . . ."
    let output = MessageParsing.ProcessMessage2(input)
    printfnT "Succesfully computed your request . . ."
    printfnT "Returning result . . ."
    Console.ForegroundColor <- ConsoleColor.Cyan 
    match output with 
    | Text (text, quick_replies, metadata) -> Console.WriteLine (sprintf "Bab-Bot>: %s" text)
    | _ -> ()  
  
  System.Console.ReadLine() |> ignore

  0
