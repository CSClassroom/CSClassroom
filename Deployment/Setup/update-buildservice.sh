pushd Playbooks
export TERRAFORM_STATE_ROOT=../../Provision
ansible-playbook -u root -i terraform.py update-buildservice.yml
popd
