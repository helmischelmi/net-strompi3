import serial
import os
from time import sleep
import RPi.GPIO as GPIO

breakS = 0.1
breakL = 0.5

GPIO.setmode(GPIO.BCM)
# Serialless pin
GPIO_TPIN = 21

#kill background python process
#change scriptname to the name of your python process running in the background that need to be killed 
os.system("sudo pkill -f scriptname.py")
sleep(1)

# disable serialless mode
GPIO.setup(GPIO_TPIN,GPIO.OUT)
GPIO.output(GPIO_TPIN, GPIO.LOW)
print ("Setting GPIO to LOW to Disable Serialless Mode.")
print ("This will take approx. 10 seconds.")
sleep(10)
GPIO.cleanup()
print ("Serialless Mode is Disabled!")




#serial port setup
serial_port = serial.Serial()

serial_port.baudrate = 38400
serial_port.port = "/dev/serial0"
serial_port.timeout = 1
serial_port.bytesize = 8
serial_port.stopbits = 1
serial_port.parity = serial.PARITY_NONE

# open serial port
if serial_port.isOpen(): serial_port.close()
serial_port.open()

while True:
    # close StromPi console
    serial_port.write(str.encode("quit"))
    sleep(breakS)
    serial_port.write(str.encode("\x0D"))
    sleep(breakL)
    # send poweroff command to StromPi
    serial_port.write(str.encode("poweroff"))
    sleep(breakS)
    serial_port.write(str.encode("\x0D"))
    # shutting down Raspberry Pi
    print("sudo shutdown -h now")
    print("Shutting down...")

    sleep(2)
    os.system("sudo shutdown -h now")