# inkomate

## .NET C# program to mange incontinence aids of residents of nursing homes

### v0.9.x - working

Improvemnts have been made, making this program almost ready for its first major release. All currently implemented functions have been working without bugs during testing - however, since this program is not fully released  yet, expect it to break rare occasions. 

### ToDo

- Add English support
- Code clean-up
- Remove generic allowance value - make it per insurance
- Add functions for insurances (add, remove, update etc.)
- Comment & explain code
- Creating MSI file 
- Reducing redundant code (e.g. database-handler)

### Problems

- ~~Depending on the system language the final .csv file might include wrongly formated text or delimiter in other languages~~
- MSI file is unusable (even of older versions prior to v0.9.X)

### Database

This project uses a SQLite database. An associated SQL file is provided in order to create the tables of this database. The database file must be named "inkomate.sqlite" and must reside within the same directory as the executable. 

### Patch notes

- Fixed several bugs 
- Added search function
- Improved sorting function
- Added QOF features (e.g. displaying the count of current residents)
- Fixed .csv cultural problems
