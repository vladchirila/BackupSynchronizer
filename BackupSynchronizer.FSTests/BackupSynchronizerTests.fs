module BackupSynchronizer.FSTests.BackupSynchronizerTests

open NUnit.Framework
open BackupSynchronizer

[<SetUp>]
let Setup () =
    ()

let mutable foldersToCleanup:string list = []

[<OneTimeTearDown>]
let Teardown () =
    foldersToCleanup |> List.iter (fun f -> System.IO.Directory.Delete(f))
    
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

let rec getStructure root =
    let directories = [for d in System.IO.Directory.EnumerateDirectories(root) do yield d]
    let files = [for f in System.IO.Directory.EnumerateFiles(root) do yield f]
    let eachDirectoryStructure = directories |> List.map getStructure |> List.fold List.append []
    (root, files) :: eachDirectoryStructure

let checkStructureContains root (structure: (string * string list) list) =
    let existingStructure = set (getStructure root)
    let expectedStructure = set (structure |> List.map (fun (x, y) -> (System.IO.Path.Combine (root,x), y |> List.map (fun z -> System.IO.Path.Combine (root,x,z)))))
    let existingNotExpected = Set.difference existingStructure expectedStructure |> Set.toList
    let expectedNotExisting = Set.difference expectedStructure existingStructure |> Set.toList
    if existingNotExpected.Length = 0 && expectedNotExisting.Length = 0 then
        true
    else
        printfn "existing not expected: %A" existingNotExpected
        printfn "expected not existing: %A" expectedNotExisting
        false

[<Test>]
let ``Sursa ramane la fel dupa backup`` () =
    let sourceStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let destStructure = [("", []); ("s", ["s.1"]); ("s\\1", ["s1.1";"s1.2";"s1.3"]); ("s\\2", []); ("s\\3", []); ("s\\4", ["a.a";"a.b";"a.c"]);]
    let sourceRootPath = createStructure sourceStructure
    let destRootPath = createStructure destStructure
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,false)
    Assert.IsTrue(checkStructureContains sourceRootPath sourceStructure)
    
[<Test>]
let ``Destinatia e cum trebuie dupa backup`` () =
    let sourceStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let destStructure = [("", []); ("s", ["s.1"]); ("s\\1", ["s1.1";"s1.2";"s1.3"]); ("s\\2", []); ("s\\3", []); ("s\\4", ["a.a";"a.b";"a.c"]);]
    let sourceRootPath = createStructure sourceStructure
    let destRootPath = createStructure destStructure
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,false)
    Assert.IsTrue(checkStructureContains destRootPath sourceStructure)
    
[<Test>]
let ``Daca destinatia e goala atunci se creaza structura din sursa`` () =
    let structure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let sourceRootPath = createStructure structure
    let destRootPath = getTempPath ()
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,false)
    Assert.IsTrue(checkStructureContains destRootPath structure)
        
[<Test>]
let ``Daca destinatia e la fel cu sursa atunci ramane la fel`` () =
    let structure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let sourceRootPath = createStructure structure
    let destRootPath = createStructure structure
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,false)
    Assert.IsTrue(checkStructureContains destRootPath structure)
    
[<Test>]
let ``Daca lipseste un folder atunci e creat`` () =
    let sourceStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let destStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []);]
    let sourceRootPath = createStructure sourceStructure
    let destRootPath = createStructure destStructure
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,false)
    Assert.IsTrue(checkStructureContains destRootPath sourceStructure)

[<Test>]
let ``Daca lipseste un fisier e creat`` () =
    let sourceStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let destStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let sourceRootPath = createStructure sourceStructure
    let destRootPath = createStructure destStructure
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,false)
    Assert.IsTrue(checkStructureContains destRootPath sourceStructure)
    
[<Test>]
let ``Daca e un fisier in plus e sters`` () =
    let sourceStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let destStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c";"a.d"]);]
    let sourceRootPath = createStructure sourceStructure
    let destRootPath = createStructure destStructure
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,false)
    Assert.IsTrue(checkStructureContains destRootPath sourceStructure)

[<Test>]
let ``Daca e un folder in plus e sters`` () =
    let sourceStructure = [("", []); ("s", ["s.1";"s.2"]);]
    let destStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]);]
    let sourceRootPath = createStructure sourceStructure
    let destRootPath = createStructure destStructure
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,false)
    Assert.IsTrue(checkStructureContains destRootPath sourceStructure)
 
[<Test>]
let ``Daca e un fisier in plus si nu stergem, atunci nu e sters`` () =
    let sourceStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c"]);]
    let destStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]); ("s\\2", []); ("s\\3", []); ("s\\3\\a", ["a.a";"a.b";"a.c";"a.d"]);]
    let sourceRootPath = createStructure sourceStructure
    let destRootPath = createStructure destStructure
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,true)
    Assert.IsTrue(checkStructureContains destRootPath destStructure)

[<Test>]
let ``Daca e un folder in plus si nu stergem, atunci nu e sters`` () =
    let sourceStructure = [("", []); ("s", ["s.1";"s.2"]);]
    let destStructure = [("", []); ("s", ["s.1";"s.2"]); ("s\\1", ["s1.1";"s1.2"]);]
    let sourceRootPath = createStructure sourceStructure
    let destRootPath = createStructure destStructure
    foldersToCleanup <- List.append [sourceRootPath; destRootPath] foldersToCleanup
    (new BackupSynchronizer.BackupSynchronizerAction ()).DoBackup (sourceRootPath,destRootPath,true)
    Assert.IsTrue(checkStructureContains destRootPath destStructure)
    