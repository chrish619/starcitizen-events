import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

  render () {
    return (
      <div>
        <h1>Hello, world!</h1>
        <ul>
          <li><a href='https://github.com/chrish619/starcitizen-events'>Source</a> for this site</li>
          <li><a href='https://docs.google.com/spreadsheets/d/10GTXs6nvbTATYvdgYkFBUiHnox1-j1pHpCKcjVKgRMM'>SC Tools.</a> The Source spreadsheet, curated / maintained by Bognogus</li>
          <li><a href='https://twitch.tv/bognogus'>Bognogus!</a></li>
        </ul>
      </div>
    );
  }
}
