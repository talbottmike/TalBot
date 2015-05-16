type IPlug =
   abstract member Run: unit -> string
   
type plug () =
    interface IPlug with
        member x.Run(): string = 
            "foo"

let p = new plug()

let p1 = p :> IPlug
let p2 = p :> obj
let p3 = p2 :?> IPlug

printfn "%s" (p3.Run ())