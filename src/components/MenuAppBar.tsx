import React from "react";
import { createStyles, makeStyles, Theme } from "@material-ui/core/styles";
import { Link } from "react-router-dom";

// Bar
import AppBar from "@material-ui/core/AppBar";
import Toolbar from "@material-ui/core/Toolbar";
import Typography from "@material-ui/core/Typography";

// Menu burger
import MenuIcon from "@material-ui/icons/Menu";
import IconButton from "@material-ui/core/IconButton";

import clsx from "clsx";

// Used in the draw list
import Drawer from "@material-ui/core/Drawer";
import List from "@material-ui/core/List";
import Divider from "@material-ui/core/Divider";
import ListItem from "@material-ui/core/ListItem";
import ListItemText from "@material-ui/core/ListItemText";

// Icons for the navigation menu
import StarIcon from "@material-ui/icons/Star";
import EqualizerIcon from "@material-ui/icons/Equalizer";
import SettingsIcon from "@material-ui/icons/Settings";
import LockIcon from "@material-ui/icons/Lock";
import HourglassFullIcon from "@material-ui/icons/HourglassFull";

const useStyles = makeStyles((theme: Theme) =>
  createStyles({
    root: {
      flexGrow: 1,
    },
    menuButton: {
      marginRight: theme.spacing(2),
    },
    title: {
      flexGrow: 1,
    },
    list: {
      width: 250,
    },
    fullList: {
      width: "auto",
    },
    drawLogo: {
      maxWidth: "50%",
    },
    drawLogoWrapper: {
      background: "rgba(0,0,0, .1)",
    },
  })
);

export default function MenuAppBar() {
  const classes = useStyles();
  // const iOS = process.browser && /iPad|iPhone|iPod/.test(navigator.userAgent);

  const drawer_side = "right";

  // First part of the draw list
  const routes = [
    {
      description: "Timeføring",
      route: "/hours",
      icon: HourglassFullIcon,
    },
    {
      description: "Admin",
      route: "/admin",
      icon: SettingsIcon,
    },
    {
      description: "Project",
      route: "/project",
      icon: StarIcon,
    },
    {
      description: "Rapportering",
      route: "/reports",
      icon: EqualizerIcon,
    },
  ];

  // Botton part of the draw list
  const actions = [
    {
      description: "Logg av",
      action: () => {
        console.log("Logging out");
      },
      icon: LockIcon,
    },
  ];

  const [state, setState] = React.useState(false);

  // Open and close drawer
  const toggleDrawer = (anchor: string, open: boolean) => (event: any) => {
    if (
      event.type === "keydown" &&
      (event.key === "Tab" || event.key === "Shift")
    ) {
      return;
    }
    setState(open);
  };

  // Drawer list
  const list = (anchor: string) => (
    <div
      role="presentation"
      className={clsx(classes.list, {
        [classes.fullList]: anchor === "top" || anchor === "bottom",
      })}
      onClick={toggleDrawer(anchor, false)}
      onKeyDown={toggleDrawer(anchor, false)}
    >
      <Link to="/" className={"centerContent " + classes.drawLogoWrapper}>
        <img
          alt="Alv logo"
          className={classes.drawLogo}
          src={process.env.PUBLIC_URL + "/img/logo192.png"}
        />
      </Link>

      <List>
        {routes.map((item, index) => (
          <Link to={item.route}>
            <ListItem button key={item.description}>
              <item.icon className={classes.menuButton} />
              <ListItemText primary={item.description} />
            </ListItem>
          </Link>
        ))}
      </List>
      <Divider />
      <List>
        {actions.map((item, index) => (
          <ListItem button key={item.description} onClick={item.action}>
            {<item.icon className={classes.menuButton} />}
            <ListItemText primary={item.description} />
          </ListItem>
        ))}
      </List>
    </div>
  );

  // Main template
  return (
    <div className={classes.root}>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" className={classes.title}>
            Alvtime Admin
          </Typography>
          <div>
            <IconButton
              edge="start"
              className={classes.menuButton}
              color="inherit"
              aria-label="menu"
              onClick={toggleDrawer(drawer_side, true)}
            >
              <MenuIcon />
            </IconButton>

            <React.Fragment key={drawer_side}>
              <Drawer
                anchor={drawer_side}
                open={state}
                onClose={toggleDrawer(drawer_side, false)}
              >
                {list(drawer_side)}
              </Drawer>
            </React.Fragment>
          </div>
        </Toolbar>
      </AppBar>
    </div>
  );
}
