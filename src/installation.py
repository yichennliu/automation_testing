import os
import shutil
import socket

import win32file
from usb_drive import locate_usb

usb_drive = locate_usb()[0]
flash_disk_qtionaire = usb_drive + "copy_TestingDay_docs"
flash_disk_installer = usb_drive + "copy_TestingDay_installer"
desktop = os.path.join(os.path.join(os.environ['USERPROFILE']), 'Desktop')

#copy paper questionaire from usb drive
def copy_paper_qstnaire(source):
    shutil.copytree(source, desktop + "\TestingDay2_docs")

def copy_installer(setup):
    shutil.copytree(setup, desktop + "\TestingDay2_installer")

# delete older data to reinstall
def delete_centerdata():
    prevdata = "C:\\ProgramData\CentERdata"
    shutil.rmtree(prevdata, ignore_errors=True)

def copy_rename():
    index = ''
    source = r"C:\ProgramData\CentERdata"
    laptop_name = socket.gethostname()
    dest = usb_drive + laptop_name + "_centERdata_"
    while True:
        try:
            shutil.copytree(source, dest + index)
            break

        except WindowsError:
            if index:
                index = '(' + str(int(index[1:-1]) + 1) + ')'
            else:
                index = '(1)'
            pass

def locate_usb():
    drive_list = []
    drivebits = win32file.GetLogicalDrives()
    for d in range(1, 26):
        mask = 1 << d
        if drivebits & mask:
            # here if the drive is at least there
            drname = '%c:\\' % chr(ord('A') + d)
            t = win32file.GetDriveType(drname)
            if t == win32file.DRIVE_REMOVABLE:
                drive_list.append(drname)
    return (drive_list)


if __name__ == '__main__':
    copy_paper_qstnaire(flash_disk_qtionaire)
    copy_installer(flash_disk_installer)
