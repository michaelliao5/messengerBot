namespace FacebookMessengerConnector

open SendAPI
open ApiAiSDK
open ApiAiSDK.Model
open System
open canopy
open System.Text.RegularExpressions
open runner

module MessageParsing =
  open FSharp.Data
  open Newtonsoft.Json
  open System.Text.RegularExpressions

  let aiToken = new AIConfiguration("6adee404ca74426e9476a9e56acd32f9", SupportedLanguage.English);
  let apiAi = new ApiAi(aiToken)

  let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
        else None

  let booktitleRun (bookTitle:string) : string= 
    url (sprintf "https://jet.com/search?term=%s" <| bookTitle.Replace(" ", "%20"))
    click ".products-right"
    let price = read ".*-size-medium.a-color-price.header-price"
    let isbn = elementWithText ".h5" "ISBN13"
    let ss = read isbn
    let s = ss.Split([|' ';'\n'|]).[1]
    s

  let getISBN (bookTitle:string) : string = 
    start chrome
    let s = booktitleRun(bookTitle)
    quit()
    s

  let ProcessMessage (input:string) : Message = 
    match input with    
    | Regex @"look up book (.*)" [ bookTitle ] ->
        let isbn = getISBN bookTitle

        Message.Text(isbn, None, None)
    | _ -> Message.Text(apiAi.TextRequest(input).Result.Fulfillment.Speech, None, None)

