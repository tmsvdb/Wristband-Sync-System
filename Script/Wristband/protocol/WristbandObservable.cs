
using System.Collections.Generic;

namespace Wristband {

    public class WristbandObservable {
        List<IWristbandObserver> observers = new List<IWristbandObserver>();

        public void SubscibeObserver (IWristbandObserver observer) {
            observers.Add(observer);
        }

        public void UnsubscibeObserver (IWristbandObserver observer) {
            observers.Remove(observer);
        }

        public List<IWristbandObserver> SubscribtionList { get { return observers; } }

        /*
            WRISTBAND EVENTS
        */
        protected void OnInitComplete () {
            foreach (IWristbandObserver observer in observers) {
                observer.OnBluetoothInitialized();
            }
        }
        protected void OnDeInitComplete () {
            foreach (IWristbandObserver observer in observers) {
                observer.OnBluetoothDeInitialized();
            }
        }
        protected void OnERROR (WristbandProtocolError error, string info) {
            foreach (IWristbandObserver observer in observers) {
                observer.OnError(error, info);
            }
        }
        protected void OnDebugMSG (string msg) {
            foreach (IWristbandObserver observer in observers) {
                observer.OnWristbandDebugMessage(msg);
            }
        }
        protected void OnConnected (string pheriperalID, string pheriperalName, WristbandProfile profile) {
            foreach (IWristbandObserver observer in observers) {
                observer.OnWristbandConnected(pheriperalID, pheriperalName, profile);
            }
        }
        protected void OnDisconnected () {
            foreach (IWristbandObserver observer in observers) {
                observer.OnWristbandDisconnected();
            }
        }
        protected void OnMatchingProfile (string pheriperalID, string pheriperalName) {
            foreach (IWristbandObserver observer in observers) {
                observer.OnWristbandProfileFound(pheriperalID, pheriperalName);
            }
        }
        protected void OnUnknownProfile (string pheriperalID, string pheriperalName) {
            foreach (IWristbandObserver observer in observers) {
                observer.OnWristbandUnknownProfile(pheriperalID, pheriperalName);
            }
        }
        protected void OnStepsCollected (StepsData stepsData) {
            foreach (IWristbandObserver observer in observers) {
                observer.OnWristbandStepsCollected(stepsData);
            }
        }
        protected void OnWriteComplete () {
            foreach (IWristbandObserver observer in observers) {
                observer.OnWristbandWriteComplete();
            }
        }
    }
}