module BackupSynchronizer.FSTests.FileSystemActionsTests

open NUnit.Framework
open BackupSynchronizer

let mutable foldersToCleanup:string list = []

[<OneTimeTearDown>]
let Teardown () =
    foldersToCleanup |> List.iter (fun f -> System.IO.Directory.Delete(f,true))
 
let getTempPath () =
    let tempPath = System.IO.Path.GetTempPath() + System.Guid.NewGuid().ToString()
    foldersToCleanup <- tempPath :: foldersToCleanup
    tempPath

[<Test>]
let ``Daca vrem sa stergem un folder read-only, se va sterge`` () =
    let tempDirPath = getTempPath ()
    let di = System.IO.Directory.CreateDirectory tempDirPath
    di.Attributes <- di.Attributes + System.IO.FileAttributes.ReadOnly;
    let fsActions = new FileSystemActions ()
    let diNode = new FolderNode (new System.IO.DirectoryInfo (tempDirPath))
    Assert.DoesNotThrow (fun () -> fsActions.Remove diNode)
    
[<Test>]
let ``Daca vrem sa stergem un fisier read-only, se va sterge`` () =
    let tempDirPath = getTempPath ()
    System.IO.Directory.CreateDirectory tempDirPath |> ignore
    let tempFilePath = System.IO.Path.Combine (tempDirPath,"a.a")
    let file = new System.IO.FileInfo (tempFilePath)
    use fs = file.Create()
    fs.Close ()
    file.Attributes <- file.Attributes + System.IO.FileAttributes.ReadOnly;
    let fsActions = new FileSystemActions ()
    let fiNode = new FileNodeElement (new System.IO.FileInfo (tempFilePath))
    Assert.DoesNotThrow (fun () -> fsActions.Remove fiNode)