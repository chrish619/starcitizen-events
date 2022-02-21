import React, { Component } from 'react';
import moment from 'moment';
import 'moment-timezone';

export class EventsCalendar extends Component {
  static displayName = EventsCalendar.name;

  constructor(props) {
    super(props);

    const offset = moment().utcOffset();
    const offHours = Math.floor(Math.abs(offset) / 60), offMin = Math.abs(offset) % 60;
    const offFormat = `${offset < 0 ? '-' : '+'}${offHours < 10 ? '0' : ''}${offHours}:${offMin < 10 ? '0' : ''}${offMin}`;

    this.state = { events: [], loading: true, showAsUtc: false, offsetFormatted: offFormat, offset: offset };
    this.toggleUtc = this.toggleUtc.bind(this);
  }

  componentDidMount() {
    this.populateEventsData();
    this.timerId = setInterval(
      () => this.populateEventsData(),
      60 * 1000
    );
  }

  componentWillUnmount() {
    clearInterval(this.timerId);
  }

  toggleUtc() {
    console.log('toggle!');
    this.setState((prev) => {
      return { showAsUtc: !prev.showAsUtc };
    });
  }

  renderTimeZoneToggle(showAsUtc) {
    return (<p>
      <button onClick={this.toggleUtc} className={showAsUtc ? 'btn btn-primary active' : 'btn btn-primary'}>
        Showing Times as {showAsUtc ? 'UTC' : moment.tz.guess()}
      </button>
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
            <tr key={event.id} className={event.startTime.isBefore(moment()) ? 'table-success' : ''}>
              <td>{event.eventName}</td>
              <td>{showAsUtc ? event.startTime.utc().format('LLLL') : event.startTime.local().format('LLLL')}</td>
              <td>{showAsUtc ? event.endTime.utc().format('LLLL') : event.endTime.local().format('LLLL')}</td>
              {event.startTime.isBefore(moment()) ? (<td>{event.endTime.fromNow()}</td>) : (<td>{event.duration.locale('en').humanize()}</td>)}
            </tr>
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
    const response = await fetch('events');
    const data = await response.json();
    this.setState((prev) => {
      return {
        events: data.map(d => {
          return {
            id: d.eventName + d.startTime,
            eventName: d.eventName,
            startTime: moment(d.startTime),
            endTime: moment(d.endTime),
            duration: moment.duration(moment(d.endTime).diff(d.startTime)),
          };
        }),
        loading: false
      };
    });
  }
}
