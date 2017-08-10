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

[<EntryPoint>]
let main argv =   
  while true do
    let input = Console.ReadLine()
    let output = MessageParsing.ProcessMessage(input)
    match output with 
    | Text (text, quick_replies, metadata) -> Console.WriteLine text
    | _ -> ()  

  
  System.Console.ReadLine() |> ignore

  0
