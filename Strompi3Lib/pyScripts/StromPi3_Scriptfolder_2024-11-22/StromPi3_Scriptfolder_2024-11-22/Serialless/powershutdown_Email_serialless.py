
#!/usr/bin/env python
# -*- coding: utf-8 -*-
import RPi.GPIO as GPIO
import time
import os
import smtplib
from email.mime.text import MIMEText

# GPIO and email configuration
GPIO.setmode(GPIO.BCM)
GPIO_TPIN = 21  # GPIO pin for power detection
ShutdownTimer = 10  # Time in seconds to wait before shutdown if power isn't restored
Restart_Mail = 1  # 1 to enable restart email notification, 0 to disable

# Email server configuration
SERVER = 'SMTP.Beispiel.DE'
PORT = 587
EMAIL = 'Beispiel@abc.de'
PASSWORD = 'Passwort'
RECIPIENTS = ['Empfänger1@abc.de' , 'Empfänger2@abc.com']  # List of email recipients

# Email subjects
SUBJECT_POWER_FAIL = 'Raspberry Pi Power Failure!'
SUBJECT_POWER_BACK = 'Raspberry Pi Power Restored!'
SUBJECT_RESTART = 'Raspberry Pi Restarted!'

# Initialize GPIO pin
GPIO.setup(GPIO_TPIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)

# Email sending function
def send_email(subject, body):
    try:
        session = smtplib.SMTP(SERVER, PORT)
        session.starttls()
        session.login(EMAIL, PASSWORD)
        msg = MIMEText(body, 'html')
        msg['Subject'] = subject
        msg['From'] = EMAIL
        msg['To'] = ", ".join(RECIPIENTS)
        session.sendmail(EMAIL, RECIPIENTS, msg.as_string())
        session.quit()
    except Exception as e:
        print(f"Failed to send email: {e}")

# Email bodies
BODY_POWER_FAIL = "<html><body><h3>Power failure detected on Raspberry Pi!</h3></body></html>"
BODY_POWER_BACK = "<html><body><h3>Power restored on Raspberry Pi!</h3></body></html>"
BODY_RESTART = "<html><body><h3>Raspberry Pi has restarted.</h3></body></html>"


# Function called on power failure detection
def power_failure_callback(channel):
    print("Power failure detected!")
    send_email(SUBJECT_POWER_FAIL, BODY_POWER_FAIL)
    
    # Start countdown to shutdown
    for remaining_time in range(ShutdownTimer, 0, -1):
        if GPIO.input(GPIO_TPIN) == 1:  # Power restored during countdown
            print("Power restored during countdown.")
            send_email(SUBJECT_POWER_BACK, BODY_POWER_BACK)
            time.sleep(2)  # Buffer time for stabilization
            return  # No need to reinitialize setup here
        print(f"Shutting down in {remaining_time} seconds if power is not restored.")
        time.sleep(1)
    
    # Proceed with shutdown if power is not restored
    print("Initiating shutdown due to prolonged power failure.")
    os.system("sudo shutdown -h now")

# Setup GPIO event detection for power failure
def detect_event_on_falling():
    GPIO.cleanup()  # Reset GPIO settings
    GPIO.setmode(GPIO.BCM)
    GPIO.setup(GPIO_TPIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)
    GPIO.add_event_detect(GPIO_TPIN, GPIO.FALLING, callback=power_failure_callback, bouncetime=300)
    print("Monitoring for power failure events...")
    while True:
        time.sleep(0.1)

# Main function to start monitoring
if __name__ == "__main__":
    if Restart_Mail == 1:
        send_email(SUBJECT_RESTART, BODY_RESTART)
    
    try:
        detect_event_on_falling()
    except KeyboardInterrupt:
        print("Keyboard Interrupt detected. Cleaning up GPIO...")
    finally:
        GPIO.cleanup()
        print("GPIO cleanup completed.")

