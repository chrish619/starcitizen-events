import moment from 'moment';
import * as signalR from '@microsoft/signalr';

export default class EventsData {
  constructor() {
    this._initConnection();
  }

  _initConnection() {
    if (this._connection) {
      return;
    }

    this._connection = new signalR.HubConnectionBuilder()
      .withUrl('events-hub')
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (ctx) => {
          return 5000;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this._connection.start();
  }

  static parseAndSort(eventData) {
    const interpolated = eventData.map(d => {
      return {
        id: d.eventName + d.startTime,
        eventName: d.eventName,
        startTime: moment(d.startTime),
        endTime: moment(d.endTime),
        duration: moment.duration(moment(d.endTime).diff(d.startTime)),
      };
    });

    interpolated.sort((a, b) => a.startTime.diff(b.startTime));

    return interpolated;
  }

  async getCalendarEvents() {
    const response = await fetch('events');
    const data = response.json();

    return EventsData.parseAndSort(data);
  }

  subscribe(onNext, onError, onCompleted) {
    const mappedCall = (events) => {
      onNext(EventsData.parseAndSort(events));
    };

    this._connection.on('PushAll', mappedCall);

    return () => {
      this._connection.off('PushAll', mappedCall);
    }
  }
}