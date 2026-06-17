# Employee Attendance System

A robust desktop application built with C# and Windows Forms for managing employee records and tracking daily attendance. 

## Features

* **Secure Access:** Hardcoded administrator login portal.
* **Employee Management (CRUD):** Add, view, edit, and delete employee records with built-in protection against deleting employees with active attendance history.
* **Attendance Tracking:** Prevent duplicate entries and easily mark employees as Present, Absent, or on Leave.
* **Individual History:** Instantly view a specific employee's complete attendance record.
* **Visual Dashboard:** Real-time, dynamic bar chart displaying overall attendance statistics.
* **Smart Search:** Filter employees by name or specific department.

## Technology Stack

* **Frontend:** C# / .NET Windows Forms
* **Backend:** ADO.NET
* **Database:** Microsoft SQL Server (LocalDB)

## How to Run the Project

To evaluate this project locally, the database must be configured first.

1. Clone or download this repository and extract the files.
2. Locate the **`Database_Setup.sql`** file in the root directory.
3. Open Visual Studio and navigate to **SQL Server Object Explorer**.
4. Connect to `(localdb)\MSSQLLocalDB`.
5. Open a new query window, paste the contents of `Database_Setup.sql`, and execute it to automatically build the `EmployeeAttendanceDB` database and necessary tables.
6. Open the `.sln` file in Visual Studio.
7. Click **Start** to run the application.

**Test Credentials:**
* **Username:** admin
* **Password:** 1234
