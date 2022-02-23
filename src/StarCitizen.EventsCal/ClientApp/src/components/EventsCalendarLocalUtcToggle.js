import React, { Component } from 'react';
import moment from 'moment';
import 'moment-timezone';

export class EventsCalendarLocalUtcToggle extends Component {
  static displayName = EventsCalendarLocalUtcToggle.name;

  constructor(props) {
    super(props);

    this.toggle = this.toggle.bind(this);
  }

  componentDidMount() {
  }

  componentWillUnmount() {
  }

  toggle() {
    this.props.toggle();
  }

  render() {
    const timezone = this.props.utcEnabled ? 'UTC' : moment.tz.guess();

    return (
      <button onClick={this.toggle} className={this.props.utcEnabled ? 'btn btn-secondary' : 'btn btn-primary'}>
        Showing Times as {timezone}
      </button>
    );
  }
}
