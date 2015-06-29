module Regex

open System.Text.RegularExpressions

/// Get all matches for a regex pattern
let getMatches pattern input =
    Regex.Matches(input,pattern,RegexOptions.IgnoreCase) 
    |> Seq.cast
    |> Seq.map (fun (regMatch:Match) -> regMatch.Value)
