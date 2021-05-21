module BackupSynchronizer.FSTests

open NUnit.Framework
open BackupSynchronizer

[<SetUp>]
let Setup () =
    ()
    
let getTempPath () =
    System.IO.Path.GetTempPath() + System.Guid.NewGuid().ToString()

let createStructure (structure: (string * string list) list) =
    let root = getTempPath ()
    structure |> List.iter (fun (folder, files) ->
        let folderPath = System.IO.Path.Combine (root,folder)
        System.IO.Directory.CreateDirectory (folderPath) |> ignore
        files |> List.iter (fun file ->
            let filePath = System.IO.Path.Combine (folderPath,file)
            use file = System.IO.File.Create (filePath)
            file |> ignore
        )
    )
    root

    //TODO: asta trebuie modificat, ca nu verifica daca nu sunt si fisiere sau foldere extra
let checkStructureContains root (structure: (string * string list) list) =
    let mutable success = true
    structure |> List.iter (fun (folder,files) ->
        let folderPath = System.IO.Path.Combine (root,folder)
        if not (System.IO.Directory.Exists (folderPath)) then success <- false
        else 
        files |> List.iter (fun file ->
            let filePath = System.IO.Path.Combine (folderPath,file)
            if not (System.IO.File.Exists (filePath)) then success <- false
        )
    )
    success

[<Test>]
let ``Daca destinatia e goala atunci se creaza structura din sursa`` () =
    let structure = [("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let sourceRootPath = createStructure structure
    let destRootPath = getTempPath ()
    BackupSynchronizer.BackupSynchronizerAction.DoBackup (sourceRootPath,destRootPath, false)
    Assert.IsTrue(checkStructureContains destRootPath structure)
