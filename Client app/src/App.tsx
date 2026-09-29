import { BrowserRouter } from 'react-router-dom'
import { AppShell } from './app/AppShell'
import { AuthProvider } from './features/auth'
import './App.scss'

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppShell />
      </AuthProvider>
    </BrowserRouter>
  )
}

export default App
