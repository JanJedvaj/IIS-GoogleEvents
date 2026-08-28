import { AppBar, Box, Button, Chip, Container, Toolbar, Typography } from '@mui/material';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/useAuth';

export function Layout() {
  const { username, isAdmin, signOut } = useAuth();
  const navigate = useNavigate();

  const handleSignOut = async () => {
    await signOut();
    navigate('/login', { replace: true });
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppBar position="static">
        <Toolbar sx={{ gap: 2 }}>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            IIS Google Events
          </Typography>
          <Button component={NavLink} to="/events" color="inherit">
            Events
          </Button>
          <Button component={NavLink} to="/graphql" color="inherit">
            GraphQL
          </Button>
          {isAdmin && (
            <Button component={NavLink} to="/import" color="inherit">
              Import
            </Button>
          )}
          <Chip
            label={isAdmin ? 'Admin' : 'User'}
            color={isAdmin ? 'secondary' : 'default'}
            size="small"
          />
          <Typography variant="body2">{username}</Typography>
          <Button color="inherit" onClick={handleSignOut}>
            Sign out
          </Button>
        </Toolbar>
      </AppBar>
      <Container component="main" sx={{ flexGrow: 1, py: 3 }}>
        <Outlet />
      </Container>
    </Box>
  );
}
