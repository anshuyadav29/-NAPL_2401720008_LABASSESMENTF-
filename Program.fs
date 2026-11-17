open System
open Microsoft.Data.Sqlite
type Student = {
    Id: int
    Name: string
    Age: int
    Course: string
    Marks: int
}
let connectionString =
    "Data Source=students.db"

let createTable () =
    task {
        use connection =
            new SqliteConnection(connectionString)

        do! connection.OpenAsync()

        use command = connection.CreateCommand()

        command.CommandText <-
            """
            CREATE TABLE IF NOT EXISTS Students (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Age INTEGER NOT NULL,
                Course TEXT NOT NULL,
                Marks INTEGER NOT NULL
            )
            """

        let! _ = command.ExecuteNonQueryAsync()
        return ()
    }
let addStudent student =
    task {
        use connection =
            new SqliteConnection(connectionString)

        do! connection.OpenAsync()

        use command = connection.CreateCommand()

        command.CommandText <-
            """
            INSERT INTO Students
            (Id, Name, Age, Course, Marks)
            VALUES
            ($id, $name, $age, $course, $marks)
            """

        command.Parameters.AddWithValue("$id", student.Id) |> ignore
        command.Parameters.AddWithValue("$name", student.Name) |> ignore
        command.Parameters.AddWithValue("$age", student.Age) |> ignore
        command.Parameters.AddWithValue("$course", student.Course) |> ignore
        command.Parameters.AddWithValue("$marks", student.Marks) |> ignore

        let! _ = command.ExecuteNonQueryAsync()

        printfn "Student added (new member added)"
    }
let getStudents () =
    task {
        use connection =
            new SqliteConnection(connectionString)

        do! connection.OpenAsync()

        use command = connection.CreateCommand()

        command.CommandText <-
            "SELECT Id, Name, Age, Course, Marks FROM Students"

        use! reader = command.ExecuteReaderAsync()

        let students = ResizeArray<Student>()

        let mutable reading = true

        while reading do
            let! hasRow = reader.ReadAsync()

            if hasRow then
                students.Add({
                    Id = reader.GetInt32(0)
                    Name = reader.GetString(1)
                    Age = reader.GetInt32(2)
                    Course = reader.GetString(3)
                    Marks = reader.GetInt32(4)
                })
            else
                reading <- false

        return students |> Seq.toList
    }
let updateStudent id newMarks =
    task {
        use connection =
            new SqliteConnection(connectionString)

        do! connection.OpenAsync()

        use command = connection.CreateCommand()

        command.CommandText <-
            """
            UPDATE Students
            SET Marks = $marks
            WHERE Id = $id
            """

        command.Parameters.AddWithValue("$marks", newMarks) |> ignore
        command.Parameters.AddWithValue("$id", id) |> ignore

        let! result = command.ExecuteNonQueryAsync()

        if result > 0 then
            printfn "Student updated (team up )"
        else
            printfn "no student"
    }

let deleteStudent id =
    task {
        use connection =
            new SqliteConnection(connectionString)

        do! connection.OpenAsync()

        use command = connection.CreateCommand()

        command.CommandText <-
            "DELETE FROM Students WHERE Id = $id"

        command.Parameters.AddWithValue("$id", id) |> ignore

        let! result = command.ExecuteNonQueryAsync()

        if result > 0 then
            printfn "byee to the kid"
        else
            printfn "Student not found(lost)"
    }
let processStudents students =
    let toppers =
        students
        |> List.filter (fun s -> s.Marks >= 70)
    let names =
        students
        |> List.map (fun s -> s.Name)

    printfn "\n70 or 70+:"
    toppers |> List.iter (fun s ->
        printfn "%s - %d marks" s.Name s.Marks
    )

    printfn "\nStudent names:"
    names |> List.iter (printfn "%s")

let checkResult student =

    match student.Marks with
    | marks when marks >= 80 ->
        printfn "%s: very very very good" student.Name

    | marks when marks >= 60 ->
        printfn "%s: Good kiddddd" student.Name

    | marks when marks >= 40 ->
        printfn "%s: Pass pass pass partyy" student.Name

    | _ ->
        printfn "%s: sad....you are fail" student.Name

[<EntryPoint>]
let main argv =

    task {

        do! createTable()

        let student1 = {
            Id = 1
            Name = "arvind kejriwal"
            Age = 20
            Course = "AAP"
            Marks = 55
        }

        let student2 = {
            Id = 2
            Name = "modi"
            Age = 21
            Course = "BJP"
            Marks = 75
        }

        let student3 = {
            Id = 3
            Name = "Ambani"
            Age = 19
            Course = "Jio"
            Marks = 99
        }
        do! addStudent student1
        do! addStudent student2
        do! addStudent student3

        printfn "\nALL STUDENTS are hereeee"

        let! students = getStudents()

        students |> List.iter (fun s ->
            printfn "ID: %d | Name: %s | Age: %d | Course: %s | Marks: %d"
                s.Id s.Name s.Age s.Course s.Marks
        )

        printfn "\n MAP AND FILTER "

        processStudents students
        printfn "\nRESULTS (parinam)"

        students |> List.iter checkResult

        // admission open
        printfn "\nUPDATE STUDENT"

        do! updateStudent 1 95
        let! updatedStudents = getStudents()

        updatedStudents |> List.iter (fun s ->
            printfn "%s - %d number" s.Name s.Marks
        )
        //Transfer Certificate  tc
        printfn "\n DELETE STUDENT"

        do! deleteStudent 3

        //leftover strudents
        printfn "\nFINAL RECORDS"

        let! finalStudents = getStudents()

        finalStudents |> List.iter (fun s ->
            printfn "%d - %s - %s - %d"
                s.Id s.Name s.Course s.Marks
        )

        printfn "\n Byeeee!"
    }
        }.GetAwaiter().GetResult()

    0