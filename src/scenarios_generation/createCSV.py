import sqlite3,csv

db = sqlite3.connect("input/AuditTrailData.db")

def input_items (db, table_name):
    c = db.cursor()
    c.execute("SELECT * FROM " + table_name)
    rows = c.fetchall()
    return rows

def csv_output(rows):
    for count in range(1,3):
        csv_file_name = "csv_files/output_" + str(count) + ".csv"
        with open(csv_file_name, 'w') as f:
            writer = csv.writer(f)
            writer.writerows(rows)
        f.close()


AuditSessionData = input_items(db, "AuditSessionData")
EventData = input_items(db, "EventData")

auditSessionData_csv = csv_output(AuditSessionData)
eventData_csv= csv_output(EventData)



