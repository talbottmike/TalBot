module TalBot.Agent.ArgumentParser

open TalBot

type ArgumentCommand = Run | RunService | ShowHelp | InstallService | UninstallService

// set up a type to represent the options
type CommandLineOptions = {
    serviceName: string;
    command: ArgumentCommand;
    directoryToUpgrade: string
    filteredArguments: string
    }

let (|Prefix|_|) (p:string) (s:string) =
    if s.StartsWith(p) then
        Some <| s.[1..]
    else
        None
        
let rec parseCommandLineRec args optionsSoFar = 
    match args with 
    // empty list means we're done.
    | [] -> 
        optionsSoFar

    | h::t & (Prefix "/" h1::_ | Prefix "-" h1::_) ->
        match h1::t with
        // empty list means we're done.
        | [] -> optionsSoFar

        // match name by flag
        | "n"::xs -> 
            //start a submatch on the next arg
            match xs with
            | h::xss -> 
                let newOptionsSoFar = { optionsSoFar with serviceName=h; filteredArguments=sprintf "%s -n %s" optionsSoFar.filteredArguments h }
                parseCommandLineRec xss newOptionsSoFar 

            // unrecognized option set command to show help and keep looping
            | _ -> 
                let newOptionsSoFar = { optionsSoFar with command=ShowHelp}
                parseCommandLineRec [] newOptionsSoFar 

        // match command by flag
        | "c"::xs -> 
        //start a submatch on the next arg
        match xs with
            | "s"::xss -> 
                let newOptionsSoFar = { optionsSoFar with command=RunService}
                parseCommandLineRec xss newOptionsSoFar 

            | "h"::xss -> 
                let newOptionsSoFar = { optionsSoFar with command=ShowHelp}
                parseCommandLineRec [] newOptionsSoFar 

            | "i"::xss -> 
                let newOptionsSoFar = { optionsSoFar with command=InstallService; filteredArguments=sprintf "%s -c s " optionsSoFar.filteredArguments }
                parseCommandLineRec xss newOptionsSoFar 

            | "u"::xss -> 
                let newOptionsSoFar = { optionsSoFar with command=UninstallService}
                parseCommandLineRec xss newOptionsSoFar 

            // unrecognized option set command to show help and stop looping
            | _ -> 
                let newOptionsSoFar = { optionsSoFar with command=ShowHelp}
                parseCommandLineRec [] newOptionsSoFar 

        // ignore unrecognized option and keep looping
        | x::xs -> 
            parseCommandLineRec xs optionsSoFar 

    // skip non command
    | h::t -> 
        parseCommandLineRec t optionsSoFar

// create the "public" parse function
let parseCommandLine args = 
   // create the defaults
   let defaultOptions = {
        serviceName = "TalBot"; 
        command = Run;
        directoryToUpgrade = "";
        filteredArguments = "";
       }

   // call the recursive one with the initial options
   parseCommandLineRec args defaultOptions