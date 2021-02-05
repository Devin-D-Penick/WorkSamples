import React, { Fragment } from "react";
import PropTypes from "prop-types";
import clsx from "clsx";

import logger from "sabio-debug";
import {
  Grid,
  Input,
  InputLabel,
  Checkbox,
  MenuItem,
  FormControl,
  Select,
} from "@material-ui/core";

import { makeStyles } from "@material-ui/core/styles";
const _logger = logger.extend("Locations DropDown");
const useStyles = makeStyles((theme) => ({
  formControl: {
    width: "100%",
  },
  chips: {
    display: "flex",
    flexWrap: "wrap",
  },
  chip: {
    margin: 2,
  },
  noLabel: {
    marginTop: theme.spacing(3),
  },
}));

const ITEM_HEIGHT = 48;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
  PaperProps: {
    style: {
      maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
      width: 300,
    },
  },
};

export default function LocationsDropDown(props) {
  const classes = useStyles();

  const locations = props.locations || [];

  //const locationTypeClicked = props.onChange;

  const onLocationsClicked = (event) => {
    _logger("Testing olc", event.currentTarget);
    props.onChange(event.currentTarget);
  };

  return (
    <Fragment>
      <div className="m-3">
        <FormControl className={classes.formControl}>
          <InputLabel id="demo-mutiple-name-label">Location Type</InputLabel>
          <Select
            labelId="demo-mutiple-name-label"
            id="demo-mutiple-name"
            name="locations"
            value={locations.name}
            onChange={onLocationsClicked}
            input={<Input />}
            MenuProps={MenuProps}
          >
            {locations.map((location) => (
              <MenuItem
                key={location.id}
                value={location.id}
                placeholder="Location Type"
              >
                {location.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </div>
    </Fragment>
  );
}

LocationsDropDown.propTypes = {
  onChange: PropTypes.func,

  locations: PropTypes.arrayOf(
    PropTypes.shape({
      id: PropTypes.number,
      name: PropTypes.string,
    })
  ),
};
