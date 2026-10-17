const groups = [
  {title:'UX research sprint',course:'Interaction Design',format:'In person',day:'Tue 14:00',detail:'Library collaboration room',spots:6,tag:'UX'},
  {title:'Database lab buddies',course:'Software Development',format:'Online',day:'Tue 18:00',detail:'Video call + shared notes',spots:4,tag:'SQL'},
  {title:'Python practice hour',course:'Software Development',format:'In person',day:'Wed 16:00',detail:'Lab 2',spots:8,tag:'Python'},
  {title:'Cyber fundamentals',course:'Cyber Security',format:'Online',day:'Thu 19:00',detail:'Video call + quiz',spots:5,tag:'Security'},
  {title:'Accessibility critique',course:'Interaction Design',format:'In person',day:'Fri 13:00',detail:'Design studio',spots:7,tag:'WCAG'},
  {title:'Portfolio build session',course:'Interaction Design',format:'Online',day:'Sat 10:00',detail:'Focus room + feedback',spots:3,tag:'Portfolio'}
];
const events = [
  {title:'Career clinic: CV basics',category:'Careers',when:'Wed 17:30',place:'Student success centre',desc:'Bring a draft CV and practise turning experience into clear evidence.'},
  {title:'Study skills workshop',category:'Academic',when:'Thu 12:30',place:'Learning commons',desc:'Plan a realistic week, break down tasks and build a simple study routine.'},
  {title:'Campus games afternoon',category:'Social',when:'Fri 15:00',place:'Main quad',desc:'A low-pressure social afternoon with board games and team activities.'},
  {title:'Internship Q&A',category:'Careers',when:'Mon 18:00',place:'Online session',desc:'Hear how students can prepare for internships and ask practical questions.'}
];
const resources = [
  {title:'Library and research support',cat:'Academic',desc:'Find databases, referencing help and study-space information.',action:'Open library guide'},
  {title:'Accessibility support',cat:'Student support',desc:'Learn how to request accessible learning support and assistive technology.',action:'View support options'},
  {title:'Academic skills centre',cat:'Academic',desc:'Writing, study planning and assignment support in plain language.',action:'See available sessions'},
  {title:'Careers and employability',cat:'Careers',desc:'CV reviews, interview practice, internship resources and events.',action:'Explore careers'},
  {title:'Student finance information',cat:'Student support',desc:'A starting point for finding official information about fees and funding.',action:'View information'},
  {title:'Student clubs directory',cat:'Social',desc:'Discover clubs, societies and ways to meet people with shared interests.',action:'Browse clubs'}
];
const state = { joined:new Set(), rsvps:new Set(), tasks:[{text:'Finish database lab',done:true},{text:'Join UX study group',done:false},{text:'Attend careers workshop',done:false}] };
function escapeHtml(str){return str.replace(/[&<>'"]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[c]));}
function renderGroups(){
  const course=document.getElementById('groupCourse').value, format=document.getElementById('groupFormat').value, q=document.getElementById('groupSearch').value.toLowerCase();
  const filtered=groups.filter(g=>(course==='all'||g.course===course)&&(format==='all'||g.format===format)&&(`${g.title} ${g.course} ${g.tag}`.toLowerCase().includes(q)));
  document.getElementById('groupList').innerHTML=filtered.map(g=>`<article class="content-card"><div class="card-top"><span class="badge">${escapeHtml(g.course)}</span><span class="badge">${escapeHtml(g.tag)}</span></div><h3>${escapeHtml(g.title)}</h3><p>${escapeHtml(g.detail)}</p><div class="meta"><span>${escapeHtml(g.format)}</span><span>${escapeHtml(g.day)}</span><span>${g.spots} spots</span></div><div class="card-action"><button class="btn ${state.joined.has(g.title)?'btn-secondary':'btn-primary'} join-btn" type="button" data-group="${escapeHtml(g.title)}">${state.joined.has(g.title)?'Joined - leave':'Join group'}</button></div></article>`).join('');
  document.getElementById('groupEmpty').classList.toggle('hidden',filtered.length>0);
  document.querySelectorAll('.join-btn').forEach(btn=>btn.addEventListener('click',()=>{const name=btn.dataset.group;if(state.joined.has(name)){state.joined.delete(name);btn.textContent='Join group';btn.classList.remove('btn-secondary');btn.classList.add('btn-primary');}else{state.joined.add(name);btn.textContent='Joined - leave';btn.classList.remove('btn-primary');btn.classList.add('btn-secondary');}}));
}
function renderEvents(){
  const cat=document.getElementById('eventCategory').value;
  document.getElementById('eventList').innerHTML=events.filter(e=>cat==='all'||e.category===cat).map(e=>`<article class="content-card"><div class="card-top"><span class="badge">${escapeHtml(e.category)}</span><span class="badge">${escapeHtml(e.when)}</span></div><h3>${escapeHtml(e.title)}</h3><p>${escapeHtml(e.desc)}</p><div class="meta"><span>${escapeHtml(e.place)}</span></div><div class="card-action"><button class="btn ${state.rsvps.has(e.title)?'btn-secondary':'btn-primary'} event-btn" type="button" data-event="${escapeHtml(e.title)}">${state.rsvps.has(e.title)?'RSVP confirmed - cancel':'RSVP'}</button></div></article>`).join('');
  document.querySelectorAll('.event-btn').forEach(btn=>btn.addEventListener('click',()=>{const name=btn.dataset.event;if(state.rsvps.has(name)){state.rsvps.delete(name);btn.textContent='RSVP';btn.classList.add('btn-primary');btn.classList.remove('btn-secondary');}else{state.rsvps.add(name);btn.textContent='RSVP confirmed - cancel';btn.classList.add('btn-secondary');btn.classList.remove('btn-primary');}}));
}
function renderResources(){
  const q=document.getElementById('resourceSearch').value.toLowerCase();
  document.getElementById('resourceList').innerHTML=resources.filter(r=>`${r.title} ${r.cat} ${r.desc}`.toLowerCase().includes(q)).map(r=>`<article class="content-card"><div class="card-top"><span class="badge">${escapeHtml(r.cat)}</span></div><h3>${escapeHtml(r.title)}</h3><p>${escapeHtml(r.desc)}</p><div class="card-action"><button class="btn btn-secondary resource-btn" type="button" data-resource="${escapeHtml(r.title)}">${escapeHtml(r.action)}</button></div></article>`).join('');
  document.querySelectorAll('.resource-btn').forEach(btn=>btn.addEventListener('click',()=>{alert(`${btn.dataset.resource}: demo action only in this prototype.`);}));
}
function renderTasks(){
  const list=document.getElementById('taskList');
  list.innerHTML=state.tasks.map((t,i)=>`<div class="task-row ${t.done?'done':''}"><input type="checkbox" id="task-${i}" ${t.done?'checked':''} data-index="${i}"><label for="task-${i}">${escapeHtml(t.text)}</label><button class="delete-task" type="button" data-delete="${i}" aria-label="Delete ${escapeHtml(t.text)}">Delete</button></div>`).join('');
  const done=state.tasks.filter(t=>t.done).length; document.getElementById('progressPill').textContent=`${done} of ${state.tasks.length} complete`;
  list.querySelectorAll('input[type=checkbox]').forEach(cb=>cb.addEventListener('change',()=>{state.tasks[Number(cb.dataset.index)].done=cb.checked;renderTasks();}));
  list.querySelectorAll('[data-delete]').forEach(btn=>btn.addEventListener('click',()=>{state.tasks.splice(Number(btn.dataset.delete),1);renderTasks();}));
}
function routeTo(route, moveFocus=false){
  document.querySelectorAll('.route').forEach(el=>el.classList.add('hidden'));
  const target=document.getElementById(`${route}Route`); if(target) target.classList.remove('hidden');
  document.querySelectorAll('[data-route]').forEach(a=>a.removeAttribute('aria-current'));
  document.querySelectorAll(`[data-route="${route}"]`).forEach(a=>a.setAttribute('aria-current','page'));
  if(moveFocus) document.getElementById('main').focus({preventScroll:true});
  document.getElementById('primary-nav').classList.remove('open');
}
function setupAccessibility(){
  const dialog=document.getElementById('accessibilityDialog'); document.getElementById('accessibilityBtn').addEventListener('click',()=>dialog.showModal()); document.getElementById('closeAccessibility').addEventListener('click',()=>dialog.close());
  document.getElementById('largeTextToggle').addEventListener('change',e=>document.body.classList.toggle('large-text',e.target.checked));
  document.getElementById('contrastToggle').addEventListener('change',e=>document.body.classList.toggle('high-contrast',e.target.checked));
  document.getElementById('motionToggle').addEventListener('change',e=>document.body.classList.toggle('reduced-motion',e.target.checked));
}
document.addEventListener('click',e=>{const link=e.target.closest('[data-route]'); if(link){e.preventDefault();const route=link.dataset.route;history.pushState({route},'',`#${route}`);routeTo(route,true);}});
window.addEventListener('popstate',()=>routeTo(location.hash.replace('#','')||'home',true));
document.getElementById('groupCourse').addEventListener('change',renderGroups);document.getElementById('groupFormat').addEventListener('change',renderGroups);document.getElementById('groupSearch').addEventListener('input',renderGroups);document.getElementById('eventCategory').addEventListener('change',renderEvents);document.getElementById('resourceSearch').addEventListener('input',renderResources);
document.querySelectorAll('[data-mood]').forEach(btn=>btn.addEventListener('click',()=>{document.getElementById('moodStatus').textContent=`You selected “${btn.dataset.mood}”. Try your next small step in My plan.`;}));
document.getElementById('taskForm').addEventListener('submit',e=>{e.preventDefault();const input=document.getElementById('newTask');state.tasks.push({text:input.value.trim(),done:false});input.value='';renderTasks();});
document.querySelector('.nav-toggle').addEventListener('click',e=>{const nav=document.getElementById('primary-nav');const open=nav.classList.toggle('open');e.currentTarget.setAttribute('aria-expanded',String(open));});
renderGroups();renderEvents();renderResources();renderTasks();setupAccessibility();routeTo(location.hash.replace('#','')||'home');