const t_btn = document.getElementById('t-btn');
const r_btn = document.getElementById('r-btn');
const d_btn = document.getElementById('d-btn');
const l_btn = document.getElementById('auth-link');

apiGet(`/Database/users/${authEmail}/role`).then(function(result){
  let role = result.data;
  if(role!="operator"){
    d_btn.style["display"] = "none";
  }
});


if(authEmail==""){
  t_btn.style["display"] = "none";
  r_btn.style["display"] = "none";
  d_btn.style["display"] = "none";
}

if(authEmail!=""){
  l_btn.innerHTML = 'Выход';
  l_btn.onclick = function(){
      setCookie("Uni-Email", "", 90);
      setCookie("Uni-Password", "", 90);
      authEmail = "";
      authPassword = "";
  };
  l_btn.href = './index.html';
}
