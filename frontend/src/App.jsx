import React, { useState } from 'react';
import { User, Mail, Phone, Hash, Users, Home, Building2, Lock } from 'lucide-react';

const App = () => {
  const [page, setPage] = useState('landing');
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({
    name: '',
    regNumber: '',
    mobile: '',
    vitEmail: '',
    personalEmail: '',
    teamNumber: '',
    codename: '',
    password: '',
    residenceType: 'dayscholar',
    hostelType: '',
    blockRoom: ''
  });
  const [people, setPeople] = useState([]);
  const [selectedPerson, setSelectedPerson] = useState('');
  const [reviewText, setReviewText] = useState('');
  const [rating, setRating] = useState(5);
  const [myReviews, setMyReviews] = useState([]);

  // load regNumber from localStorage if present
  React.useEffect(() => {
    const stored = localStorage.getItem('regNumber');
    if (stored) setFormData((s) => ({ ...s, regNumber: stored }));
  }, []);

  // load people when entering the dashboard
  React.useEffect(() => {
    if (page === 'main') {
      (async () => {
        const reg = formData.regNumber || localStorage.getItem('regNumber');
        const res = await fetch(`/api/reviews/people?excludeRegNumber=${encodeURIComponent(reg || '')}`);
        if (res.ok) {
          const data = await res.json();
          setPeople(data);
        } else {
          setPeople([]);
        }
        // also load user's reviews
        await loadMyReviews();
      })();
    }
  }, [page]);

  const loadMyReviews = async () => {
    const reg = formData.regNumber || localStorage.getItem('regNumber');
    if (!reg) return;
    const res = await fetch(`/api/reviews/?subjectRegNumber=${encodeURIComponent(reg)}`);
    if (res.ok) {
      const data = await res.json();
      setMyReviews(data);
    }
  };

  const handleInputChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async () => {
    const endpoint = isLogin ? '/api/auth/login' : '/api/auth/signup';
    
    try {
      // Build payloads with both camelCase (frontend aliases) and snake_case
      // keys so the backend checks (which sometimes use snake_case) will match.
      let payload;
      if (isLogin) {
        payload = {
          regNumber: formData.regNumber,
          reg_number: formData.regNumber,
          password: formData.password
        };
      } else {
        payload = {
          name: formData.name,
          regNumber: formData.regNumber,
          reg_number: formData.regNumber,
          mobile: formData.mobile,
          vitEmail: formData.vitEmail,
          vit_email: formData.vitEmail,
          personalEmail: formData.personalEmail,
          personal_email: formData.personalEmail,
          teamNumber: formData.teamNumber,
          team_number: formData.teamNumber,
          codename: formData.codename,
          password: formData.password,
          residenceType: formData.residenceType,
          residence_type: formData.residenceType,
          hostelType: formData.hostelType,
          hostel_type: formData.hostelType,
          blockRoom: formData.blockRoom,
          block_room: formData.blockRoom
        };
      }

      const response = await fetch(endpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      
      const data = await response.json();
      
      if (response.ok) {
        localStorage.setItem('token', data.token);
        setPage('main');
        // keep regNumber (login input) in state/localStorage for subsequent calls
        localStorage.setItem('regNumber', formData.regNumber);
      } else {
        alert(data.detail || 'Authentication failed');
      }
    } catch (error) {
      alert('Server connection error. Please ensure FastAPI backend is running on port 8000.');
    }
  };

  if (page === 'landing') {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-900 via-purple-900 to-pink-900 flex items-center justify-center p-4">
        <div className="text-center">
          <h1 className="text-7xl font-bold text-white mb-8 animate-pulse">LAND</h1>
          <button
            onClick={() => setPage('auth')}
            className="bg-white text-purple-900 px-8 py-4 rounded-lg text-xl font-semibold hover:bg-gray-100 transition-all transform hover:scale-105 shadow-2xl"
          >
            Login / Signup
          </button>
        </div>
      </div>
    );
  }

  if (page === 'main') {
    // Review dashboard: Add Review / View Reviews
    return (
      <div className="min-h-screen bg-gradient-to-br from-green-900 via-teal-900 to-blue-900 flex items-start justify-center p-8">
        <div className="w-full max-w-4xl bg-white rounded-2xl shadow-2xl p-6">
          <div className="flex justify-between items-center mb-6">
            <div>
              <h1 className="text-3xl font-bold text-gray-800">Reviews Dashboard</h1>
              <p className="text-sm text-gray-600">Logged in as {formData.regNumber || 'Unknown'}</p>
            </div>
            <div>
              <button
                onClick={() => {
                  localStorage.removeItem('token');
                  setPage('landing');
                  setFormData({
                    name: '', regNumber: '', mobile: '', vitEmail: '', personalEmail: '',
                    teamNumber: '', codename: '', password: '', residenceType: 'dayscholar', hostelType: '', blockRoom: ''
                  });
                }}
                className="bg-red-500 text-white px-4 py-2 rounded-md font-semibold hover:bg-red-600 transition-all"
              >
                Logout
              </button>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-6">
            <div className="col-span-1">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-xl font-semibold">Add Review</h2>
                <button
                  onClick={async () => {
                    // fetch people
                    const res = await fetch(`/api/reviews/people?excludeRegNumber=${encodeURIComponent(formData.regNumber)}`);
                    if (res.ok) {
                      const data = await res.json();
                      setPeople(data);
                    } else {
                      alert('Failed to load people');
                    }
                  }}
                  className="text-sm text-blue-600"
                >Refresh list</button>
              </div>

              <div className="mb-3">
                <label className="block text-sm text-gray-700 mb-1">People</label>
                <div className="border rounded-md max-h-60 overflow-auto">
                  {people.length === 0 && <div className="p-3 text-gray-600">No people found.</div>}
                  {people.map(p => (
                    <div key={p.regNumber} className={`p-3 flex items-center justify-between hover:bg-gray-50 ${selectedPerson === p.regNumber ? 'bg-gray-100' : ''}`}>
                      <div>
                        <div className="font-medium text-gray-800">{p.name}</div>
                        <div className="text-sm text-gray-500">{p.regNumber}</div>
                      </div>
                      <div>
                        <button onClick={() => setSelectedPerson(p.regNumber)} className="px-3 py-1 bg-blue-600 text-white rounded-md text-sm">{selectedPerson === p.regNumber ? 'Selected' : 'Select'}</button>
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              <div className="mb-3">
                <label className="block text-sm text-gray-700 mb-1">Review</label>
                <textarea value={reviewText} onChange={(e) => setReviewText(e.target.value)} className="w-full p-2 border rounded-md" rows={4} />
              </div>

              <div className="mb-4">
                <label className="block text-sm text-gray-700 mb-1">Rating</label>
                <input type="number" min={1} max={5} value={rating} onChange={(e) => setRating(Number(e.target.value))} className="w-24 p-2 border rounded-md" />
              </div>

              <div>
                <button
                  onClick={async () => {
                    if (!selectedPerson) return alert('Select a person');
                    if (!reviewText) return alert('Write a review');
                    const body = {
                      reviewerRegNumber: formData.regNumber,
                      subjectRegNumber: selectedPerson,
                      content: reviewText,
                      rating: rating
                    };
                    const res = await fetch('/api/reviews/', {
                      method: 'POST',
                      headers: { 'Content-Type': 'application/json' },
                      body: JSON.stringify(body)
                    });
                    if (res.status === 201) {
                      alert('Review posted');
                      setReviewText('');
                      setRating(5);
                    } else {
                      const err = await res.json().catch(() => ({}));
                      alert(err.detail || 'Failed to post review');
                    }
                  }}
                  className="bg-green-600 text-white px-4 py-2 rounded-md font-semibold hover:bg-green-700"
                >Post Review</button>
              </div>
            </div>

            <div className="col-span-1">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="text-xl font-semibold">View Reviews About You</h2>
                <button onClick={async () => { await loadMyReviews(); }} className="text-sm text-blue-600">Refresh</button>
              </div>

              <div className="space-y-3 max-h-[420px] overflow-auto">
                {myReviews.length === 0 && <p className="text-gray-600">No reviews for the current month.</p>}
                {myReviews.map(r => (
                  <div key={r.id} className="p-3 border rounded-md">
                    <p className="text-gray-800">{r.content}</p>
                    <p className="text-sm text-gray-500">Rating: {r.rating}</p>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-900 via-purple-800 to-pink-800 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl p-8">
        <div className="flex justify-center mb-6">
          <div className="bg-gray-100 rounded-lg p-1 flex">
            <button
              onClick={() => setIsLogin(true)}
              className={`px-6 py-2 rounded-md font-semibold transition-all ${
                isLogin ? 'bg-purple-600 text-white' : 'text-gray-600'
              }`}
            >
              Login
            </button>
            <button
              onClick={() => setIsLogin(false)}
              className={`px-6 py-2 rounded-md font-semibold transition-all ${
                !isLogin ? 'bg-purple-600 text-white' : 'text-gray-600'
              }`}
            >
              Signup
            </button>
          </div>
        </div>

        <h2 className="text-3xl font-bold text-center mb-8 text-gray-800">
          {isLogin ? 'Welcome Back' : 'Create Account'}
        </h2>

        <div className="space-y-4">
          {isLogin ? (
            <>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">VIT Email</label>
                <div className="relative">
                  <Mail className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="email"
                    name="vitEmail"
                    value={formData.vitEmail}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="your.email@vitstudent.ac.in"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Registration Number</label>
                <div className="relative">
                  <Hash className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="text"
                    name="regNumber"
                    value={formData.regNumber}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="21BCE1234"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Password</label>
                <div className="relative">
                  <Lock className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="password"
                    name="password"
                    value={formData.password}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="Enter your password"
                  />
                </div>
              </div>
            </>
          ) : (
            <>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Name</label>
                <div className="relative">
                  <User className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="text"
                    name="name"
                    value={formData.name}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="John Doe"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Registration Number</label>
                <div className="relative">
                  <Hash className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="text"
                    name="regNumber"
                    value={formData.regNumber}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="21BCE1234"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Mobile Number</label>
                <div className="relative">
                  <Phone className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="tel"
                    name="mobile"
                    value={formData.mobile}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="+91 98765 43210"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">VIT Email</label>
                <div className="relative">
                  <Mail className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="email"
                    name="vitEmail"
                    value={formData.vitEmail}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="your.email@vitstudent.ac.in"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Personal Email</label>
                <div className="relative">
                  <Mail className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="email"
                    name="personalEmail"
                    value={formData.personalEmail}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="personal@gmail.com"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Team Number on Merch</label>
                <div className="relative">
                  <Users className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="text"
                    name="teamNumber"
                    value={formData.teamNumber}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="Team 42"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Codename</label>
                <div className="relative">
                  <User className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="text"
                    name="codename"
                    value={formData.codename}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="Phoenix"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Password</label>
                <div className="relative">
                  <Lock className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                  <input
                    type="password"
                    name="password"
                    value={formData.password}
                    onChange={handleInputChange}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    placeholder="Create a password"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Residence Type</label>
                <select
                  name="residenceType"
                  value={formData.residenceType}
                  onChange={handleInputChange}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                >
                  <option value="dayscholar">Day Scholar</option>
                  <option value="hosteller">Hosteller</option>
                </select>
              </div>

              {formData.residenceType === 'hosteller' && (
                <>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Hostel Type</label>
                    <select
                      name="hostelType"
                      value={formData.hostelType}
                      onChange={handleInputChange}
                      className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                    >
                      <option value="">Select Type</option>
                      <option value="mh">Men's Hostel (MH)</option>
                      <option value="lh">Ladies Hostel (LH)</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Block and Room Number</label>
                    <div className="relative">
                      <Building2 className="absolute left-3 top-3 h-5 w-5 text-gray-400" />
                      <input
                        type="text"
                        name="blockRoom"
                        value={formData.blockRoom}
                        onChange={handleInputChange}
                        className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                        placeholder="A-Block, Room 301"
                      />
                    </div>
                  </div>
                </>
              )}
            </>
          )}

          <button
            onClick={handleSubmit}
            className="w-full bg-gradient-to-r from-purple-600 to-pink-600 text-white py-3 rounded-lg font-semibold hover:from-purple-700 hover:to-pink-700 transition-all transform hover:scale-105 shadow-lg"
          >
            {isLogin ? 'Login' : 'Sign Up'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default App;