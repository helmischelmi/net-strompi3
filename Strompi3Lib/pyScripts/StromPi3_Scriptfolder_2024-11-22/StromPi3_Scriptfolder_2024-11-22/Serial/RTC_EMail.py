#!/usr/bin/env python
# -*- coding: utf-8 -*-
import RPi.GPIO as GPIO
import time
from time import sleep
import datetime
import serial
import os
import smtplib
from email.mime.text import MIMEText
GPIO.setmode(GPIO.BCM)


#Here you can choose whether you want to receive an email when the Raspberry Pi restarts - 1 to activate - 0 to deactivate
Restart_Mail = 1

# This is The config for the EMAIL notification
#----------------------------------------------
SERVER =    'SMTP.Beispiel.DE'
PORT =      587
EMAIL =    'Beispiel@abc.de'
PASSWORT =  'Passwort'
EMPFAENGER =    ['Empfänger1@abc.de' , 'Empfänger2@abc.com']
SUBJECT_Powerfail =   'Raspberry Pi Powerfail!'
SUBJECT_Powerback =   'Raspberry Pi Powerback!'
SUBJECT_Restart =     'Raspberry Pi Restart!'
#----------------------------------------------



serial_port = serial.Serial(
 port='/dev/serial0',
 baudrate = 38400,
 parity=serial.PARITY_NONE,
 stopbits=serial.STOPBITS_ONE,
 bytesize=serial.EIGHTBITS,
 timeout=3
)

if serial_port.isOpen(): serial_port.close()
serial_port.open()




serial_port.write(str.encode('Q'))
sleep(1)
serial_port.write(str.encode('\x0D'))
sleep(1)
serial_port.write(str.encode('date-rpi'))
sleep(0.1)
serial_port.write(str.encode('\x0D'))
data = serial_port.read(9999);
date = int(data)


strompi_year = date // 10000
strompi_month = date % 10000 // 100
strompi_day = date % 100

sleep(0.1)
serial_port.write(str.encode('time-rpi'))
sleep(0.1)
serial_port.write(str.encode('\x0D'))
data = serial_port.read(9999);
timevalue = int(data)

strompi_hour = timevalue // 10000
strompi_min = timevalue % 10000 // 100
strompi_sec = timevalue % 100

rpi_time = datetime.datetime.now().replace(microsecond=0)
strompi_time = datetime.datetime(2000 + strompi_year, strompi_month, strompi_day, strompi_hour, strompi_min, strompi_sec, 0)

command = 'set-time %02d %02d %02d' % (int(rpi_time.strftime('%H')),int(rpi_time.strftime('%M')),int(rpi_time.strftime('%S')))



if rpi_time > strompi_time:
    serial_port.write(str.encode('set-date %02d %02d %02d %02d') % (int(rpi_time.strftime('%d')),int(rpi_time.strftime('%m')),int(rpi_time.strftime('%Y'))%100,int(rpi_time.isoweekday())))
    sleep(0.5)
    serial_port.write(str.encode('\x0D'))
    sleep(1)
    serial_port.write(str.encode('set-clock %02d %02d %02d') % (int(rpi_time.strftime('%H')),int(rpi_time.strftime('%M')),int(rpi_time.strftime('%S'))))
    sleep(0.5)
    serial_port.write(str.encode('\x0D'))

    print ("-----------------------------------------")
    print ("The date und time has been synced: Raspberry Pi -> StromPi")
    print ("-----------------------------------------")

else:
    os.system('sudo date +%%y%%m%%d --set=%02d%02d%02d' % (strompi_year, strompi_month, strompi_day))
    os.system('sudo date +%%T -s "%02d:%02d:%02d"' % (strompi_hour, strompi_min, strompi_sec))
    print ("-----------------------------------------")
    print ("The date und time has been synced: StromPi -> Raspberry Pi")
    print ("-----------------------------------------")
	
	
	
if serial_port.isOpen(): serial_port.close()
serial_port.open()
	



print ("E-Mail notification in  case of Powerfailure (CTRL-C for exit)")
# Set pin as input


def Sendmail_Restart():
	BODY =      """
	<html>
	<head></head>
	<body>
	<style type="text/css">
	.tg  {border-collapse:collapse;border-spacing:0;}
	.tg td{font-family:Arial, sans-serif;font-size:14px;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}
	.tg th{font-family:Arial, sans-serif;font-size:14px;font-weight:normal;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}
	.tg .tg-0ord{text-align:right}
	.tg .tg-qnmb{font-weight:bold;font-size:16px;text-align:center}
	</style>
	<table class="tg">
	<tr>
	<th class="tg-qnmb" colspan="2">Ihr Raspberry Pi wurde neugestartet.</th>
	</tr>
	</table>
	</body>
	</html>
	"""

	session = smtplib.SMTP(SERVER, PORT)
	session.set_debuglevel(1)
	session.ehlo()
	session.starttls()
	session.ehlo
	session.login(EMAIL, PASSWORT)
	msg = MIMEText(BODY, 'html')
	msg['Subject'] = SUBJECT_Restart
	msg['From'] = EMAIL
	msg['To'] = ", ".join(EMPFAENGER)
	session.sendmail(EMAIL, EMPFAENGER, msg.as_string())
	Detect_Powerfail()

def Sendmail_Powerfail():
	BODY =      """
	<html>
	<head></head>
	<body>
	<style type="text/css">
	.tg  {border-collapse:collapse;border-spacing:0;}
	.tg td{font-family:Arial, sans-serif;font-size:14px;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}
	.tg th{font-family:Arial, sans-serif;font-size:14px;font-weight:normal;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}
	.tg .tg-0ord{text-align:right}
	.tg .tg-qnmb{font-weight:bold;font-size:16px;text-align:center}
	</style>
	<table class="tg">
	<tr>
	<th class="tg-qnmb" colspan="2">StromPi hat einen STROMAUSFALL erkannt!!!!</th>
	</tr>
	</table>
	</body>
	</html>
	"""

	session = smtplib.SMTP(SERVER, PORT)
	session.set_debuglevel(1)
	session.ehlo()
	session.starttls()
	session.ehlo
	session.login(EMAIL, PASSWORT)
	msg = MIMEText(BODY, 'html')
	msg['Subject'] = SUBJECT_Powerfail
	msg['From'] = EMAIL
	msg['To'] = ", ".join(EMPFAENGER)
	session.sendmail(EMAIL, EMPFAENGER, msg.as_string())
	Detect_Powerback()

def Sendmail_Powerback():
	BODY =      """
	<html>
	<head></head>
	<body>
	<style type="text/css">
	.tg  {border-collapse:collapse;border-spacing:0;}
	.tg td{font-family:Arial, sans-serif;font-size:14px;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}
	.tg th{font-family:Arial, sans-serif;font-size:14px;font-weight:normal;padding:10px 5px;border-style:solid;border-width:1px;overflow:hidden;word-break:normal;}
	.tg .tg-0ord{text-align:right}
	.tg .tg-qnmb{font-weight:bold;font-size:16px;text-align:center}
	</style>
	<table class="tg">
	<tr>
	<th class="tg-qnmb" colspan="2">StromPi hat Spannung wiedererkannt!</th>
	</tr>
	</table>
	</body>
	</html>
	"""

	session = smtplib.SMTP(SERVER, PORT)
	session.set_debuglevel(1)
	session.ehlo()
	session.starttls()
	session.ehlo
	session.login(EMAIL, PASSWORT)
	msg = MIMEText(BODY, 'html')
	msg['Subject'] = SUBJECT_Powerback
	msg['From'] = EMAIL
	msg['To'] = ", ".join(EMPFAENGER)
	session.sendmail(EMAIL, EMPFAENGER, msg.as_string())
	Detect_Powerfail()





def Detect_Powerback():
    while 1:
     x=serial_port.readline()
     y=x.decode(encoding='UTF-8',errors='strict')
     if y==('xxx--StromPiPowerBack--xxx\n'):
      print ("PowerBack - Email Sent")
      Sendmail_Powerback()



def Power_Lost(a):

	print ("Raspberry Pi Powerfail detected")
	print ("Powerfail_Email sent")
	Sendmail_Powerfail()



def Detect_Powerfail():
    while 1:
     x=serial_port.readline()
     y = x.decode(encoding='UTF-8',errors='strict')
     if y==('xxxShutdownRaspberryPixxx\n') or y==('xxx--StromPiPowerfail--xxx\n'):
      print ("PowerFail - Email Sent")
      Sendmail_Powerfail()

time.sleep(3)
if Restart_Mail == 1:
	Sendmail_Restart()

try:
	Detect_Powerfail()
	while True:	
		time.sleep(0.1)

except KeyboardInterrupt:
	print ("\nKeyboard Interrupt")
finally:
	GPIO.cleanup()
	print ("Cleaned up Pins")