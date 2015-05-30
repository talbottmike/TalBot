module TalBot.Tests

open TalBot
open NUnit.Framework

[<Test>]
let ``hello returns 42`` () =
  let result = 42 //Library.hello 42
  printfn "%i" result
  Assert.AreEqual(42,result)

[<Test>]
let ``regexMatchesMultipleTickets`` () =
    let expected = ["MYPRO-1231";"MY-1"]
    let actual = Responses.regexMatches @"\w{2,5}-\d{1,5}" "bot MYPRO-1231 MY-1"
    Assert.AreEqual(expected,actual)