pushd Playbooks
export TERRAFORM_STATE_ROOT=../../Provision
ansible-playbook -i terraform.py -s update-webapp.yml
popd
