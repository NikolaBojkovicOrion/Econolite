import { BrowserRouter } from 'react-router-dom'
import { AppShell } from './app/AppShell'
import './App.scss'

function App() {
  return (
    <BrowserRouter>
      <AppShell />
    </BrowserRouter>
  )
}

export default App
