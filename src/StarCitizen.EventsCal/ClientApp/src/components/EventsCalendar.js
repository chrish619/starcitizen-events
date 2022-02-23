import React, { Component } from 'react';
import moment from 'moment';
import 'moment-timezone';

import { EventsCalendarRowEvent } from './EventsCalendarRowEvent';
import { EventsCalendarLocalUtcToggle } from './EventsCalendarLocalUtcToggle';

import EventsData from '../services/EventsData';

export class EventsCalendar extends Component {
  static displayName = EventsCalendar.name;

  constructor(props) {
    super(props);

    const offset = moment().utcOffset();
    const offHours = Math.floor(Math.abs(offset) / 60), offMin = Math.abs(offset) % 60;
    const offFormat = `${offset < 0 ? '-' : '+'}${offHours < 10 ? '0' : ''}${offHours}:${offMin < 10 ? '0' : ''}${offMin}`;

    this.state = { events: [], loading: true, showAsUtc: false, offsetFormatted: offFormat, offset: offset };
    this.toggleUtc = this.toggleUtc.bind(this);
    this.eventsData = new EventsData();
    this.unsubscribe = () => { };
  }

  componentDidMount() {
    this.populateEventsData();
  }

  componentWillUnmount() {
    this.unsubscribe();
  }

  toggleUtc() {
    console.log('toggle!');
    this.setState((prev) => {
      return { showAsUtc: !prev.showAsUtc };
    });
  }

  renderTimeZoneToggle(showAsUtc) {
    return (<p>
      <EventsCalendarLocalUtcToggle utcEnabled={showAsUtc} toggle={this.toggleUtc} />
    </p>);
  }

  static renderEventsTable(events, showAsUtc) {
    return (
      <table className='table table-striped' aria-labelledby="tabelLabel">
        <thead>
          <tr>
            <th>Name</th>
            <th>Starts</th>
            <th>Ends</th>
            <th>Duration/Ends</th>
          </tr>
        </thead>
        <tbody>
          {events.map(event =>
            <EventsCalendarRowEvent key={event.id}
              eventName={event.eventName}
              startTime={event.startTime}
              endTime={event.endTime}
              showAsUtc={showAsUtc} />
          )}
        </tbody>
      </table>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : EventsCalendar.renderEventsTable(this.state.events, this.state.showAsUtc);

    let toggle = this.state.offset === 0
      ? <></>
      : this.renderTimeZoneToggle(this.state.showAsUtc);

    return (
      <div>
        <h1 id="tabelLabel" >Upcoming events</h1>
        <p>Your local time is currently detected as {moment.tz.guess()}, UTC{this.state.offsetFormatted}</p>
        {toggle}
        {contents}
      </div>
    );
  }

  async populateEventsData() {
    this.unsubscribe = this.eventsData.subscribe(events => {
      this.setState((prev) => {
        return {
          events: events,
          loading: false
        };
      });
    });
  }
}
