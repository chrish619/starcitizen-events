import React, { Component } from 'react';
import moment from 'moment';
import 'moment-timezone';

export class EventsCalendarRowEvent extends Component {
  static displayName = EventsCalendarRowEvent.name;

  constructor(props) {
    super(props);

    this.state = {
    };
  }

  componentDidMount() {
    this.updateTimeFlags();
    this.timerId = setInterval(() => this.updateTimeFlags(), 1000);
  }

  componentWillUnmount() {
    clearInterval(this.timerId);
  }

  updateTimeFlags() {
    this.setState((prev) => ({
      endingIn: this.props.endTime.fromNow(),
      isEndingSoon: this.props.endTime.diff(moment(), 'minutes') <= 15,
      isInProgress: this.props.startTime.isBefore(moment()),
      hasEnded: this.props.endTime.isBefore(moment()),
    }));
  }

  render() {
    /*
    * LLLL => dddd, MMMM Do YYYY LT (but year is superfluous)
    */
    const formatString = 'dddd, MMMM Do LT';

    if (this.state.hasEnded) {
      /* let's hide expired events */
      return (<></>);
    }

    const duration = moment.duration(moment(this.props.endTime).diff(this.props.startTime));

    return (
      <tr className={this.state.isEndingSoon ? 'table-warning' : this.state.isInProgress ? 'table-success' : ''}>
        <td>{this.props.eventName}</td>
        <td>{this.props.showAsUtc ? this.props.startTime.utc().format(formatString) : this.props.startTime.local().format(formatString)}</td>
        <td>{this.props.showAsUtc ? this.props.endTime.utc().format(formatString) : this.props.endTime.local().format(formatString)}</td>
        <td>{this.state.isInProgress ? (this.state.endingIn) : (duration.locale('en').humanize())}</td>
      </tr>
    );
  }
}
