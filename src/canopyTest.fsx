#r "bin/debug/canopy.dll"
#r "bin/debug/WebDriver.dll"
#r "bin/debug/WebDriver.Support.dll"

//these are similar to C# using statements
open canopy
open canopy.core
open runner
open System
open OpenQA.Selenium

let bookString = "The Death and Life of Great American Cities".Replace(" ", "+")

//start an instance of the firefox browser
start chrome
//this is how you define a test
"taking canopy for a spin" &&& fun _ ->

    let isbpUrl = sprintf "https://isbnsearch.org/search?s=%s+isbn" bookString

//    url "https://jet.com/search?term=9780679741954"
    let d = element "._Mjf"
    let test = d.FindElement <| By.ClassName("title")
    
    printfn "test element: %s" test.Text
    //assert that the element with an id of 'welcome' has
    //the text 'Welcome'
//    let test = read ".price-non-sale"
//    printfn "test string: %s" test

//run all tests
run()

printfn "press [enter] to exit"
System.Console.ReadLine() |> ignore

quit()
