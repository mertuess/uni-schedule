const dep = document.getElementById('department');
const mail = document.getElementById('mail');
const pass = document.getElementById('password');
const name = document.getElementById('name');
const engName = document.getElementById('engName');
const role = document.getElementById('role');
let oldMail = "";

getDepartments().then(function(result){
  let arr = result.data;
  for(let i = 0; i < arr.length; i++){
    const d = arr[i];
    let s = document.createElement('option');
    s.innerHTML = d.Name;
    s.value = d.Id;
    dep.appendChild(s);
  }
  getUsers().then(function(result){
    let arr = result.data;
    for(let i = 0; i < arr.length; i++){
      if(arr[i].Id != localStorage.getItem('user-to-edit')) continue;
      let u = arr[i];
      oldMail = u.Mail;
      mail.value = u.Mail;
      name.value = u.Name;
      engName.value = u.EngName;
      role.value = u.Role;
      dep.value = u.DepartmentId.toString();
    }
  });
});


function submit(){
  let new_pass = null;
  let new_dep = null;
  if(pass.value != "")
    new_pass = pass.value;

  if(dep.value != "null"){
    new_dep = parseInt(dep.value);
  }
    
  updateUser(oldMail, mail.value, new_pass, name.value, engName.value, role.value, new_dep).then(function(){
    window.location.href = './dashboard.html';
  });
}

