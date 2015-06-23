module TalBot.Extensions

open System
open System.Text
open Microsoft.FSharp.Core.Printf
open System.Reflection

type System.Exception with
    member this.ToDetailedString = 
        let sb = StringBuilder()
        let delimeter = String.replicate 50 "*"
        let newLine = Environment.NewLine
        let rec printException (exn:Exception) count =
            match exn :? TargetException && exn.InnerException <> null with
            | true -> printException (exn.InnerException) count
            | false -> 
                match count with
                | 1 -> bprintf sb "%s%s%s" exn.Message newLine delimeter
                | _ -> bprintf sb "%s%s%d)%s%s%s" newLine newLine count exn.Message newLine delimeter
                bprintf sb "%sType: %s" newLine (exn.GetType().FullName)
                // Loop through the public properties of the exception object
                // and record their values.
                exn.GetType().GetProperties()
                |> Array.iter (fun propertyInfo ->
                    // InnerException and StackTrace are captured later in the process.
                    match propertyInfo.Name with
                    | "InnerException" | "StackTrace" | "Message" | "Data" -> 
                        try
                            let value = propertyInfo.GetValue(exn, null)
                            if (value <> null)
                            then bprintf sb "%s%s: %s" newLine propertyInfo.Name (value.ToString())
                        with
                        | exn2 -> bprintf sb "%s%s: %s" newLine propertyInfo.Name exn2.Message                            
                    | _ -> ()
                )
                match exn.StackTrace with
                | null -> ()
                | _ -> 
                    bprintf sb "%s%sStackTrace%s%s%s" newLine newLine newLine delimeter newLine
                    bprintf sb "%s%s" newLine exn.StackTrace
                match exn.InnerException with
                | null -> ()
                | _ -> printException exn.InnerException (count+1)
        printException this 1
        sb.ToString()
    