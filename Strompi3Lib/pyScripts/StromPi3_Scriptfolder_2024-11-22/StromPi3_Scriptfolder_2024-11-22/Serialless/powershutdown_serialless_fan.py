#!/usr/bin/python
# -*- coding: utf-8 -*-
# Initialization
import RPi.GPIO as GPIO
import time
import os
from gpiozero import CPUTemperature, PWMLED
from time import sleep
GPIO.setmode(GPIO.BCM)

led = PWMLED(2)	# PWM-Pin (GPIO2)

startTemp = 55	# Temperature at which the fan switches on

pTemp = 4		# Proportional part
iTemp = 0.2		# Integral part

fanSpeed = 0	# Fan speed
sum = 0			# variable for i part


# Here you can choose the connected GPIO-Pin and the ShutdownTimer
GPIO_TPIN = 21
ShutdownTimer = 10

print ("Safe Shutdown in the case of Powerfailure (CTRL-C for exit)")
# Set pin as input
GPIO.setup(GPIO_TPIN,GPIO.IN,pull_up_down = GPIO.PUD_UP)

Current_State  = 1
Previous_State = 1
    
def Detect_event_GPIOFALLING():
    GPIO.remove_event_detect(GPIO_TPIN)
    GPIO.cleanup()
    GPIO.setmode(GPIO.BCM)
    GPIO.setup(GPIO_TPIN,GPIO.IN,pull_up_down = GPIO.PUD_UP)
    GPIO.add_event_detect(GPIO_TPIN, GPIO.FALLING, callback=Power_Lost, bouncetime=300)
    while True:
        time.sleep(0.1)

def Power_Lost(GPIO_TPIN):
    GPIO.remove_event_detect(GPIO_TPIN)
    GPIO.cleanup()
    GPIO.setmode(GPIO.BCM)
    GPIO.setup(GPIO_TPIN,GPIO.IN,pull_up_down = GPIO.PUD_UP)
    GPIO.add_event_detect(GPIO_TPIN, GPIO.RISING, callback=Shutdown_Interrupt, bouncetime=300)
    x=0
    print ("Raspberry Pi Powerfail detected")  #print Primary power source failed
    while x != ShutdownTimer:                  #while loop that will loop until x is = ShutdownTimer    
        time.sleep(1)                        #wait for 1 second
        x = x + 1                            #increase x by 1
        if GPIO.input(GPIO_TPIN):
          break
    if x >= ShutdownTimer:                           
        print ("Raspberry Pi Shutdown!")                                #print shutdown message
        os.system("sudo shutdown -h now")                           #give the command to shutdown the raspberry
        x=0
    
def Shutdown_Interrupt(GPIO_TPIN):
    print ("Raspberry Pi Power Back Detected!")                   #Print powerback detected
    Detect_event_GPIOFALLING()
    x=0
try:
    GPIO.add_event_detect(GPIO_TPIN, GPIO.FALLING, callback=Power_Lost, bouncetime=300)
    while True:
        cpu = CPUTemperature()		# Reading the current temperature values
        actTemp = cpu.temperature		# Current temperature as float variable
        diff = actTemp - startTemp
        sum = sum + diff
        pDiff = diff * pTemp
        iDiff = sum * iTemp
        fanSpeed = pDiff + iDiff + 35
        
        if fanSpeed > 100:
            fanSpeed = 100
        elif fanSpeed < 35:
            fanSpeed = 0
            
        if sum > 100:
            sum = 100
        elif sum < -100:
            sum = -100
        #print(str(actTemp) + "C, " + str(fanSpeed))
        led.value = fanSpeed / 100	# PWM Output
        sleep(.1)


except KeyboardInterrupt:
    print ("\nKeyboard Interrupt")
finally:
    GPIO.cleanup()
    print ("Cleaned up Pins")
