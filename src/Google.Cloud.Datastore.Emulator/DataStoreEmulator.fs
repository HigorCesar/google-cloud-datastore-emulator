module Google.Cloud.Datastore.Emulator.Core
open System.Diagnostics
open System

type EmulatorOutput = { port: int}

type DataStoreEmulator() = 
    let emulator = new Process()
    let setVars = new Process()
    let emulatorStartInfo = System.Diagnostics.ProcessStartInfo("cmd.exe", 
                                arguments = "/c gcloud beta emulators datastore start",
                                UseShellExecute = true,
                                RedirectStandardOutput = true,
                                CreateNoWindow = false)
    let setVarStartInfo = System.Diagnostics.ProcessStartInfo("cmd.exe", 
                            arguments = "/c gcloud beta emulators datastore env-init",
                            UseShellExecute = true,
                            RedirectStandardOutput = true,
                            CreateNoWindow = false)    
    member this.Start() =
        emulator.StartInfo <- emulatorStartInfo
        setVars.StartInfo <- setVarStartInfo
        emulator.Start() |> ignore
        setVars.Start() |> ignore
        let mutable port = 0
        while (not setVars.StandardOutput.EndOfStream) && port = 0 do
            let line = setVars.StandardOutput.ReadLine()
            let isHostConfig = line |> (fun (l :string) -> l.Contains("DATASTORE_EMULATOR_HOST"))
            if isHostConfig then
                port <- line.Split(':').[1] |> int
        {port = port}

    member this.Stop() =
        setVars.Dispose()
        emulator.Dispose()  

    interface IDisposable with
        member x.Dispose() = 
            x.Stop()
